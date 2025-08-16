using Pulse.Domain.Models;

namespace Pulse.Application.Commands;

public sealed record CreateTaskCommand(
    string Title,
    string Description,
    DateTime DueDateUtc
);
public static class CreateTaskCommandMapper{
    public static TaskItem NewTask(this CreateTaskCommand command, DateTime nowUtc) => new(
        Id: Guid.NewGuid(),
        Title: command.Title,
        Description: command.Description,
        DueDateUtc: command.DueDateUtc,
        Status: PulseTaskStatus.New,
        CreatedAtUtc: nowUtc,
        UpdatedAtUtc: nowUtc
    );
}