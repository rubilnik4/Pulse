using CSharpFunctionalExtensions;
using Dapper;
using Npgsql;
using Pulse.Infrastructure.Errors;

namespace Pulse.Infrastructure.Repositories;

public static class DatabaseEffects
{
    public static async Task<Result<T, DbError>> Try<T>(
        NpgsqlDataSource dataSource, Func<NpgsqlConnection, Task<T>> action)
    {
        try
        {
            await using var conn = await dataSource.OpenConnectionAsync();
            var value = await action(conn);
            return Result.Success<T, DbError>(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<T, DbError>(DbError.From(ex));
        }
    }
  
    public static async Task<UnitResult<DbError>> TryAffecting(
        NpgsqlDataSource dataSource, Func<NpgsqlConnection, Task<int>> exec,string operation)
    {
        try
        {
            await using var conn = await dataSource.OpenConnectionAsync();
            var affected = await exec(conn);
            return affected > 0
                ? UnitResult.Success<DbError>()
                : UnitResult.Failure(DbError.NoRows(operation));
        }
        catch (Exception ex)
        {
            return UnitResult.Failure(DbError.From(ex));
        }
    }
   
    public static async Task<Result<T, DbError>> TryAffecting<T>(
        NpgsqlDataSource ds, Func<NpgsqlConnection, Task<int>> exec, Func<T> onSuccess, string operation)
    {
        try
        {
            await using var conn = await ds.OpenConnectionAsync();
            var affected = await exec(conn);
            return affected > 0
                ? Result.Success<T, DbError>(onSuccess())
                : Result.Failure<T, DbError>(DbError.NoRows(operation));
        }
        catch (Exception ex)
        {
            return Result.Failure<T, DbError>(DbError.From(ex));
        }
    }
   
    public static CommandDefinition Cmd(string sql, object? args = null)
        => new(sql, args);
}