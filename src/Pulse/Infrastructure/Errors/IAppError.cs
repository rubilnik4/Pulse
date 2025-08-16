namespace Pulse.Infrastructure.Errors;

public interface IAppError
{
    string ErrorType { get; }
    string Message { get; } 
    string? Detail { get; } 
}