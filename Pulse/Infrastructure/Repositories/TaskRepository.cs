using CSharpFunctionalExtensions;
using Dapper;
using Npgsql;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;
using Pulse.Infrastructure.RowModels;

namespace Pulse.Infrastructure.Repositories;

public class TaskRepository(NpgsqlDataSource dataSource): ITaskRepository
{
    public Task<Result<Guid, DbError>> Insert(TaskItem taskItem)
    {
        const string sql = @"
            INSERT INTO tasks (id, title, description, due_date_utc, status, created_at_utc, updated_at_utc)
            VALUES (@Id, @Title, @Description, @DueDateUtc, @Status, @CreatedAtUtc, @UpdatedAtUtc);";

        return DatabaseEffects.TryAffecting(
            dataSource,
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

    public Task<Result<Maybe<TaskItem>, DbError>> Get(Guid id)
    {
        const string sql = @"
            SELECT id AS ""Id"", title AS ""Title"", description AS ""Description"",
                   due_date_utc AS ""DueDateUtc"", status AS ""Status"",
                   created_at_utc AS ""CreatedAtUtc"", updated_at_utc AS ""UpdatedAtUtc""
            FROM tasks WHERE id = @Id LIMIT 1;";

        return DatabaseEffects.Try(dataSource, async conn =>
        {
            var taskItem = await conn.QueryFirstOrDefaultAsync<TaskItem>(DatabaseEffects.Cmd(sql, new { Id = id }));
            return taskItem ?? Maybe<TaskItem>.None;
        });
    }

    public Task<Result<(IReadOnlyList<TaskItem> Items, long Total), DbError>>> List(
        PulseTaskStatus? status, int offset, int limit, PulseTaskSort sort)
    {
        var orderBy = sort switch
        {
            PulseTaskSort.DueDateDesc => "due_date_utc DESC, id DESC",
            PulseTaskSort.CreatedAtDesc => "created_at_utc DESC, id DESC",
            _ => "due_date_utc ASC, id ASC"
        };

        var sql = $@"
            SELECT
              id               AS ""Id"",
              title            AS ""Title"",
              description      AS ""Description"",
              due_date_utc     AS ""DueDateUtc"",
              status           AS ""Status"",
              created_at_utc   AS ""CreatedAtUtc"",
              updated_at_utc   AS ""UpdatedAtUtc"",
              COUNT(*) OVER()  AS ""Total""
            FROM tasks
            WHERE (@Status IS NULL OR status = @Status)
            ORDER BY {orderBy}
            LIMIT @Limit OFFSET @Offset;";

        return DatabaseEffects.Try(dataSource, async conn =>
        {
            var rows = await conn.QueryAsync<TaskRowWithTotal>(DatabaseEffects.Cmd(sql, new
            {
                Status = status,
                Limit  = limit,
                Offset = offset
            }));

            var items = rows.Select(r => TaskItem.FromDb(
                id: r.Id,
                title: r.Title,
                description: r.Description,
                dueDateUtc: EnsureUtc(r.DueDateUtc),
                status: r.Status, // уже PulseTaskStatus благодаря MapEnum
                createdAtUtc: EnsureUtc(r.CreatedAtUtc),
                updatedAtUtc: EnsureUtc(r.UpdatedAtUtc)
            )).ToArray();

            var total = rows.FirstOrDefault()?.Total ?? 0L;
            return ((IReadOnlyList<TaskItem>)items, total);
        }, ct);
    }

    public Task<UnitResult<DbError>> Update(TaskItem taskItem)
    {
        const string sql = @"
            UPDATE tasks SET
              title = @Title,
              description = @Description,
              due_date_utc = @DueDateUtc,
              status = @Status,
              updated_at_utc = @UpdatedAtUtc
            WHERE id = @Id;";

        return DatabaseEffects.TryAffecting(
            dataSource,
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

    public Task<UnitResult<DbError>> UpdateStatus(Guid id, PulseTaskStatus newStatus, DateTime updatedAtUtc, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE tasks
            SET status = @Status,
                updated_at_utc = @UpdatedAtUtc
            WHERE id = @Id;";

        return DatabaseEffects.TryAffecting(
            dataSource,
            exec: conn => conn.ExecuteAsync(
                DatabaseEffects.Cmd(sql, new
                {
                    Id = id,
                    Status = newStatus,                
                    UpdatedAtUtc = updatedAtUtc
                })),
            operation: "UPDATE_STATUS"
        );
    }

    public Task<UnitResult<DbError>> Delete(Guid id)
    {
        const string sql = @"DELETE FROM tasks WHERE id = @Id;";
        return DatabaseEffects.TryAffecting(
            dataSource,
            exec: conn => conn.ExecuteAsync(DatabaseEffects.Cmd(sql, new { Id = id })),
            operation: "DELETE"
        );
    }
}
