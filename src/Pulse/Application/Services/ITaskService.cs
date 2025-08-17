using CSharpFunctionalExtensions;
using Pulse.Application.Commands;
using Pulse.Application.Pagination;
using Pulse.Application.Queries;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Errors;

namespace Pulse.Application.Services;

public interface ITaskService
{
    Task<Result<Guid, IAppError>> Create(CreateTaskCommand command);
    Task<Result<TaskItem, IAppError>> Get(Guid id);
    Task<Result<PageResult<TaskItem>, IAppError>> List(ListTasksQuery query);
    Task<UnitResult<IAppError>> Update(UpdateTaskCommand command);
    Task<UnitResult<IAppError>> ChangeStatus(Guid id, PulseTaskStatus status);
    Task<UnitResult<IAppError>> Delete(Guid id);
    Task<Result<int, IAppError>> MarkOverdue();
}