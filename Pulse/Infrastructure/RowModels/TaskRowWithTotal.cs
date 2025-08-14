using Pulse.Application.Pagination;
using Pulse.Domain.Models;

namespace Pulse.Infrastructure.RowModels;

public sealed record TaskRowTotal(
    Guid Id,
    string Title,
    string Description,
    DateTime DueDateUtc,
    PulseTaskStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    long Total
)
{
    public static PageResult<TaskItem> ToPage(IReadOnlyCollection<TaskRowTotal> rows, int page, int size)
    {
        var total = rows.FirstOrDefault()?.Total ?? 0L;
        var items = rows.Select(row => TaskItem.FromDb(
            id:           r.Id,
            title:        r.Title,
            description:  r.Description, 
            dueDateUtc:   r.DueDateUtc, 
            status:       r.Status,
            createdAtUtc: r.CreatedAtUtc,
            updatedAtUtc: r.UpdatedAtUtc
        )).ToArray();
        
        return new PageResult<TaskItem>
        {
            Page  = page,
            Size  = size,
            Total = total,
            Items = items
        };
    }
}