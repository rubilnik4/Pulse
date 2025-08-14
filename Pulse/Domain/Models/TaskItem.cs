namespace Pulse.Domain.Models;

public record TaskItem(
    Guid Id,
    string Title,
    string Description,
    DateTime DueDateUtc,
    PulseTaskStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);