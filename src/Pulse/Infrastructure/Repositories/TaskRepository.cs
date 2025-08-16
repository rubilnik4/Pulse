using CSharpFunctionalExtensions;
using Dapper;
using Npgsql;
using Pulse.Application.Pagination;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;
using Pulse.Infrastructure.RowModels;

namespace Pulse.Infrastructure.Repositories;

public class TaskRepository(NpgsqlDataSource dataSource, ILogger<ITaskRepository> logger): ITaskRepository
{
    public Task<Result<Guid, IAppError>> Insert(TaskItem taskItem)
    {
        logger.LogDebug("Inserting task to database {TaskId})", taskItem.Id);
        const string sql = @"
            INSERT INTO tasks (id, title, description, due_date_utc, status, created_at_utc, updated_at_utc)
            VALUES (@Id, @Title, @Description, @DueDateUtc, @Status, @CreatedAtUtc, @UpdatedAtUtc);";

        return DatabaseEffects.TryAffecting(
            dataSource, logger,
            exec: conn => conn.ExecuteAsync(DatabaseEffects.Cmd(sql, new {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description ,
                DueDateUtc = taskItem.DueDateUtc,
                Status = taskItem.Status,
                CreatedAtUtc = taskItem.CreatedAtUtc,
                UpdatedAtUtc = taskItem.UpdatedAtUtc
            })),
            onSuccess: () => taskItem.Id,
            operation: "INSERT"
        );
    }

    public Task<Result<Maybe<TaskItem>, IAppError>> Get(Guid id)
    {
        logger.LogDebug("Get task from database {TaskId}", id);
        const string sql = @"
            SELECT 
               id AS ""Id"", 
               title AS ""Title"", 
               description AS ""Description"",
               due_date_utc AS ""DueDateUtc"", 
               status AS ""Status"",
               created_at_utc AS ""CreatedAtUtc"", 
               updated_at_utc AS ""UpdatedAtUtc""
            FROM tasks WHERE id = @Id LIMIT 1;";

        return DatabaseEffects.Try(dataSource, logger, async conn =>
        {
            var taskItem = await conn.QueryFirstOrDefaultAsync<TaskRow>(DatabaseEffects.Cmd(sql, new { Id = id }));
            return taskItem?.ToDomain() ?? Maybe<TaskItem>.None;
        },
        operation: "GET");
    }

    public Task<Result<PageResult<TaskItem>, IAppError>> List(
        PulseTaskStatus? status, int page, int size, PulseTaskSort sort)
    {
        logger.LogDebug("Listing tasks from database (status={Status}, page={Page}, size={Size}, sort={Sort})", 
            status, page, size, sort);
        var orderBy = sort switch
        {
            PulseTaskSort.DueDateDesc => "due_date_utc DESC",
            PulseTaskSort.CreatedAtDesc => "created_at_utc DESC",
            _ => "due_date_utc ASC"
        };

        var sql = $@"
            WITH filtered AS (
                SELECT id, title, description, due_date_utc, status, created_at_utc, updated_at_utc
                FROM tasks
                WHERE (@Status IS NULL OR status = @Status)
            )
            SELECT COUNT(*) FROM filtered;

            SELECT
              id AS ""Id"",
              title AS ""Title"",
              description AS ""Description"",
              due_date_utc AS ""DueDateUtc"",
              status AS ""Status"",
              created_at_utc AS ""CreatedAtUtc"",
              updated_at_utc AS ""UpdatedAtUtc""
            FROM filtered
            ORDER BY {orderBy}
            LIMIT @Limit OFFSET @Offset;";

        return DatabaseEffects.Try(dataSource, logger, async conn =>
        {
            var offset = (page - 1) * size;
            await using var grid = await conn.QueryMultipleAsync(DatabaseEffects.Cmd(sql, new
            {
                Status = status, Limit = size, Offset = offset
            }));

            var total = await grid.ReadSingleAsync<long>();
            var rows  = await grid.ReadAsync<TaskRow>();
            return rows.ToPage(page, size, total);
        },
        operation: "GET");
    }

    public Task<UnitResult<IAppError>> Update(TaskItem taskItem)
    {
        logger.LogDebug("Updating task to database {TaskId}", taskItem.Id);
        const string sql = @"
            UPDATE tasks SET
              title = @Title,
              description = @Description,
              due_date_utc = @DueDateUtc,
              status = @Status,
              updated_at_utc = @UpdatedAtUtc
            WHERE id = @Id;";

        return DatabaseEffects.TryAffecting(
            dataSource, logger,
            exec: conn => conn.ExecuteAsync(DatabaseEffects.Cmd(sql, new {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                DueDateUtc = taskItem.DueDateUtc,
                Status = taskItem.Status,
                UpdatedAtUtc = taskItem.UpdatedAtUtc
            })),
            operation: "UPDATE"
        );
    }

    public Task<UnitResult<DatabaseError>> UpdateStatus(Guid id, TaskStatus status, DateTime updatedAtUtc)
    {
        throw new NotImplementedException();
    }

    public Task<UnitResult<IAppError>> UpdateStatus(Guid id, PulseTaskStatus status, DateTime updatedAtUtc)
    {
        logger.LogDebug("Updating status to database TaskId={TaskId}, Status={Status}", id, status);
        const string sql = @"
            UPDATE tasks
            SET status = @Status,
                updated_at_utc = @UpdatedAtUtc
            WHERE id = @Id;";

        return DatabaseEffects.TryAffecting(
            dataSource, logger,
            exec: conn => conn.ExecuteAsync(
                DatabaseEffects.Cmd(sql, new
                {
                    Id = id,
                    Status = status,                
                    UpdatedAtUtc = updatedAtUtc
                })),
            operation: "UPDATE_STATUS"
        );
    }

    public Task<UnitResult<IAppError>> Delete(Guid id)
    {
        logger.LogDebug("Deleting task {TaskId}", id);
        const string sql = @"
            DELETE FROM tasks 
            WHERE id = @Id;";
        return DatabaseEffects.TryAffecting(
            dataSource, logger,
            exec: conn => conn.ExecuteAsync(DatabaseEffects.Cmd(sql, new { Id = id })),
            operation: "DELETE"
        );
    }
}
