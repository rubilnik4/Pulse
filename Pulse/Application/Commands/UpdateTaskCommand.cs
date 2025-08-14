using Pulse.Domain.Models;

namespace Pulse.Application.Commands;

public sealed record UpdateTaskCommand(
    Guid Id, 
    string Title, 
    string Description, 
    DateTime DueDateUtc, 
    PulseTaskStatus Status
)
{
    public static TaskItem WithUpdate(UpdateTaskCommand command, TaskItem taskItem, DateTime nowUtc) => new(
        Id: taskItem.Id,
        Title: command.Title,
        Description: command.Description,
        DueDateUtc: command.DueDateUtc,
        Status: command.Status,
        CreatedAtUtc: taskItem.CreatedAtUtc,
        UpdatedAtUtc: nowUtc
    );
}