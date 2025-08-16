namespace Pulse.Application.Pagination;

public sealed record PageResult<T>(
    int Page,
    int Size,
    long Total,
    IReadOnlyList<T> Items
);