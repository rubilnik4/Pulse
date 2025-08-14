namespace Pulse.Api.Contracts.Responses;

public sealed record PagedResponse<T>(
    int Page,
    int Size,
    long Total,
    IReadOnlyList<T> Items
);