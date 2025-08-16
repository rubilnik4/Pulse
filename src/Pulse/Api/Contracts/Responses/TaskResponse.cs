namespace Pulse.Api.Contracts.Responses;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset DueDateUtc,
    TaskStatusDto Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);