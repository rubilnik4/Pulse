namespace Pulse.Infrastructure.Errors;

public sealed record DatabaseError(string ErrorType, string Message, string? Detail = null): IAppError
{
    public const string NotFoundErrorType = "NoRowsAffected";
    
    public static DatabaseError NoRows(string op) => 
        new(NotFoundErrorType, $"{op}: 0 rows affected");
    
    public static DatabaseError From(Exception ex) => ex switch
    {
        Npgsql.PostgresException p => new DatabaseError($"Postgres:{p.SqlState}", p.MessageText, p.Detail),
        TimeoutException => new DatabaseError("Timeout", ex.Message),
        OperationCanceledException => new DatabaseError("Cancelled", ex.Message),
        Npgsql.NpgsqlException => new DatabaseError("Connection", ex.Message),
        _ => new DatabaseError("Unknown", ex.Message)
    };
}