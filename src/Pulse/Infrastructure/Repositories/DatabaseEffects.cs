using CSharpFunctionalExtensions;
using Dapper;
using Npgsql;
using Pulse.Infrastructure.Errors;

namespace Pulse.Infrastructure.Repositories;

public static class DatabaseEffects
{
    public static async Task<Result<T, IAppError>> Try<T>(
        NpgsqlDataSource dataSource, ILogger logger, Func<NpgsqlConnection, Task<T>> action, string operation)
    {
        try
        {
            await using var conn = await dataSource.OpenConnectionAsync();
            var value = await action(conn);
            return Result.Success<T, IAppError>(value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database error during {Operation}: {Message}", operation, ex.Message);
            return Result.Failure<T, IAppError>(DatabaseError.From(ex));
        }
    }
  
    public static async Task<UnitResult<IAppError>> TryAffecting(
        NpgsqlDataSource dataSource, ILogger logger, Func<NpgsqlConnection, Task<int>> exec, string operation)
    {
        try
        {
            await using var conn = await dataSource.OpenConnectionAsync();
            var affected = await exec(conn);
            return affected > 0
                ? UnitResult.Success<IAppError>()
                : UnitResult.Failure<IAppError>(DatabaseError.NoRows(operation));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database error during {Operation}: {Message}", operation, ex.Message);
            return UnitResult.Failure<IAppError>(DatabaseError.From(ex));
        }
    }
   
    public static async Task<Result<T, IAppError>> TryAffecting<T>(
        NpgsqlDataSource ds, ILogger logger, Func<NpgsqlConnection, Task<int>> exec, Func<T> onSuccess, string operation)
    {
        try
        {
            await using var conn = await ds.OpenConnectionAsync();
            var affected = await exec(conn);
            return affected > 0
                ? Result.Success<T, IAppError>(onSuccess())
                : Result.Failure<T, IAppError>(DatabaseError.NoRows(operation));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database error during {Operation}: {Message}", operation, ex.Message);
            return Result.Failure<T, IAppError>(DatabaseError.From(ex));
        }
    }
   
    public static CommandDefinition Cmd(string sql, object? args = null) => 
        new(sql, args);
}