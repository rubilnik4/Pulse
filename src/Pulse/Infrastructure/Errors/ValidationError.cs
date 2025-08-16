namespace Pulse.Infrastructure.Errors;

public sealed record ValidationError(string Code, string Message, string? Field = null): IAppError
{
    public string ErrorType => "Validation";
    public string? Detail => Field;
}