using CSharpFunctionalExtensions;
using Pulse.Application.Commands;
using Pulse.Application.Pagination;
using Pulse.Application.Queries;
using Pulse.Domain.Models;

namespace Pulse.Application.Services;

public interface ITaskService
{
    Task<Result<TaskItem>> Create(CreateTaskCommand command);
    Task<PageResult<TaskItem>> List(ListTasksQuery query);
    Task<Maybe<TaskItem>> Get(Guid id);
    Task<Result<TaskItem>> Update(UpdateTaskCommand command);
    Task<Result<TaskItem>> ChangeStatus(Guid id, PulseTaskStatus status);
    Task<Result> Delete(Guid id);
}