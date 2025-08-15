using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Pulse.Infrastructure.Errors;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Pulse.Api.Endpoints;

public static class ErrorResult
{
    private static IResult ToErrorResponse(this IAppError error) => error switch
    {
        NotFoundError notFound => Results.NotFound(new ProblemDetails
        {
            Title = "Not Found",
            Detail = notFound.Message,
            Status = StatusCodes.Status404NotFound
        }),
       
        ValidationError validation => Results.ValidationProblem(
            errors: new Dictionary<string, string[]>
            {
                { validation.Field ?? string.Empty, [validation.Message] }
            },
            title: "Validation failed"),
        
        DatabaseError database => Results.Problem(new ProblemDetails
        {
            Title = database.ErrorType,
            Detail = database.Message,
            Status = StatusCodes.Status500InternalServerError
        }),
        
        ExternalApiError ext => Results.Problem(new ProblemDetails
        {
            Title = ext.ErrorType,
            Detail = ext.Message,
            Status = StatusCodes.Status502BadGateway
        }),
       
        _ => Results.Problem(new ProblemDetails
        {
            Title = error.ErrorType,
            Detail = error.Message,
            Status = StatusCodes.Status500InternalServerError
        })
    };
    
    public static IResult Match<T>(this Result<T, IAppError> result, Func<T, IResult> onSuccess)=> 
        result.IsSuccess ? onSuccess(result.Value) : ToErrorResponse(result.Error);

    public static IResult Match(this UnitResult<IAppError> result, Func<IResult> onSuccess) => 
        result.IsSuccess ? onSuccess() : ToErrorResponse(result.Error);
}