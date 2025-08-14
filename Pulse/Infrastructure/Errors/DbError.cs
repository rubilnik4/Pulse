namespace Pulse.Infrastructure.Errors;

public sealed record DbError(string Code, string Message, string? Detail = null)
{
    public static DbError NoRows(string op) => 
        new("NoRowsAffected", $"{op}: 0 rows affected");
    
    public static DbError From(Exception ex) => ex switch
    {
        Npgsql.PostgresException p => new DbError($"PG:{p.SqlState}", p.MessageText, p.Detail),
        TimeoutException => new DbError("Timeout", ex.Message),
        OperationCanceledException => new DbError("Cancelled", ex.Message),
        Npgsql.NpgsqlException => new DbError("Connection", ex.Message),
        _ => new DbError("Unknown", ex.Message)
    };
}