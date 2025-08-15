using Pulse.Application.Pagination;
using Pulse.Domain.Models;

namespace Pulse.Infrastructure.RowModels;

public sealed record TaskRow(
    Guid Id,
    string Title,
    string Description,
    DateTime DueDateUtc,
    PulseTaskStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public static class TaskRowMapper {
    public static TaskItem ToDomain(this TaskRow row) =>new(
        Id: row.Id,
        Title: row.Title,
        Description: row.Description,
        DueDateUtc: row.DueDateUtc,
        Status: row.Status,
        CreatedAtUtc: row.CreatedAtUtc,
        UpdatedAtUtc: row.UpdatedAtUtc
    );
    
    public static PageResult<TaskItem> ToPage(this IEnumerable<TaskRow> rows, int page, int size, long total)
    {
        var array = rows.ToArray();
        var items = array.Select(row => row.ToDomain()).ToArray();
        return new PageResult<TaskItem>
        (
            Page: page,
            Size: size,
            Total: total,
            Items: items
        );
    }
}