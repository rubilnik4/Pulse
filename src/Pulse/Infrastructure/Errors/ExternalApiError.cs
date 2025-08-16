namespace Pulse.Infrastructure.Errors;

public sealed record ExternalApiError(string Provider, string Code, string Message, string? Detail = null)
    : IAppError
{
    public string ErrorType => $"External:{Provider}:{Code}";
}