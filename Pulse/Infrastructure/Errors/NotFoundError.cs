namespace Pulse.Infrastructure.Errors;

public sealed record NotFoundError(string Id, string Resource): IAppError
{
    public string ErrorType => "NotFound";
    public string Message => $"{Resource} '{Id}' not found";
    public string? Detail => null;
}