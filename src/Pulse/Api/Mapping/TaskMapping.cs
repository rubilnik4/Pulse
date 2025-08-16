using Pulse.Api.Contracts;
using Pulse.Api.Contracts.Requests;
using Pulse.Api.Contracts.Responses;
using Pulse.Application.Commands;
using Pulse.Application.Queries;
using Pulse.Domain.Models;

namespace Pulse.Api.Mapping;

public static class TaskMappings
{
    public static CreateTaskCommand ToCommand(this CreateTaskRequest request) => new(
        Title: request.Title.Trim(),
        Description: request.Description.Trim(),
        DueDateUtc: request.DueDateUtc.UtcDateTime
    );
    
    public static UpdateTaskCommand ToCommand(this UpdateTaskRequest request, Guid id) => new(
        Id: id,
        Title: request.Title.Trim(),
        Description: request.Description.Trim(),
        DueDateUtc: request.DueDateUtc.UtcDateTime,
        Status: request.Status.ToDomain()
    );
    
    public static ListTasksQuery ToQuery(this PagingQuery query)
    {
        var sortKey = query.Sort?.ToDomain() ?? PulseTaskSort.DueDateAsc;
        return new ListTasksQuery(query.Status?.ToDomain(), query.Page, query.Size, sortKey);
    }

    public static TaskResponse ToResponse(this TaskItem taskItem) => new TaskResponse
    (
        Id: taskItem.Id,
        Title: taskItem.Title,
        Description: taskItem.Description,
        DueDateUtc: new DateTimeOffset(taskItem.DueDateUtc, TimeSpan.Zero),
        Status: taskItem.Status.ToDto(),
        CreatedAtUtc: new DateTimeOffset(taskItem.CreatedAtUtc, TimeSpan.Zero),
        UpdatedAtUtc: new DateTimeOffset(taskItem.UpdatedAtUtc, TimeSpan.Zero)
    );
    
    public static PulseTaskStatus ToDomain(this TaskStatusDto status) => status switch
    {
        TaskStatusDto.New => PulseTaskStatus.New,
        TaskStatusDto.InProgress => PulseTaskStatus.InProgress,
        TaskStatusDto.Completed => PulseTaskStatus.Completed,
        TaskStatusDto.Overdue => PulseTaskStatus.Overdue,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static TaskStatusDto ToDto(this PulseTaskStatus status) => status switch
    {
        PulseTaskStatus.New => TaskStatusDto.New,
        PulseTaskStatus.InProgress => TaskStatusDto.InProgress,
        PulseTaskStatus.Completed => TaskStatusDto.Completed,
        PulseTaskStatus.Overdue => TaskStatusDto.Overdue,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
    
    private static PulseTaskSort ToDomain(this TaskSortDto sort) => sort switch
    {
        TaskSortDto.CreatedAtDesc => PulseTaskSort.CreatedAtDesc,
        TaskSortDto.DueDateAsc => PulseTaskSort.DueDateAsc,
        TaskSortDto.DueDateDesc => PulseTaskSort.DueDateDesc,
        _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
    };
}