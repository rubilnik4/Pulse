using CSharpFunctionalExtensions;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;

namespace Pulse.Infrastructure.Repositories;

public interface ITaskRepository
{
    Task<Result<Guid, DbError>> Insert(TaskItem entity);
    Task<Result<Maybe<TaskItem>, DbError>> Get(Guid id);
    Task<Result<(IReadOnlyList<TaskItem> Items, long Total), DbError>> List(
        PulseTaskStatus? status, int offset, int limit, PulseTaskSort sort);
    Task<UnitResult<DbError>> Update(TaskItem entity);
    Task<UnitResult<DbError>> UpdateStatus(Guid id, TaskStatus newStatus, DateTime updatedAtUtc);
    Task<UnitResult<DbError>> Delete(Guid id);
}