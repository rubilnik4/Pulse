using Pulse.Domain.Models;

namespace Pulse.Infrastructure.RowModels;

public sealed record TaskRow(
    Guid Id,
    string Title,
    string Description,
    DateTime DueDateUtc,
    PulseTaskStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUt
);