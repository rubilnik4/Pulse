using CSharpFunctionalExtensions;
using Pulse.Application.Pagination;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;

namespace Pulse.Infrastructure.Repositories;

public interface ITaskRepository
{
    Task<Result<Guid, IAppError>> Insert(TaskItem entity);
    Task<Result<Maybe<TaskItem>, IAppError>> Get(Guid id);
    Task<Result<PageResult<TaskItem>, IAppError>> List(PulseTaskStatus? status, int page, int size, PulseTaskSort sort);
    Task<UnitResult<IAppError>> Update(TaskItem entity);
    Task<UnitResult<IAppError>> UpdateStatus(Guid id, PulseTaskStatus status, DateTime updatedAtUtc);
    Task<UnitResult<IAppError>> Delete(Guid id);
}