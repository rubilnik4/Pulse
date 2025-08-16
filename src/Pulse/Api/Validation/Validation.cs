using System.ComponentModel.DataAnnotations;

namespace Pulse.Api.Validation;

public sealed class ValidateRequest<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.FirstOrDefault(a => a is T) is not T model)
            return await next(context);

        var validationContext = new ValidationContext(model);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, validationContext, results, validateAllProperties: true))
            return await next(context);
        
        var errors = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty), (r, m) => new { Member = m, r.ErrorMessage })
            .GroupBy(x => x.Member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage ?? "Invalid").ToArray());
        return Results.ValidationProblem(errors);
    }
}