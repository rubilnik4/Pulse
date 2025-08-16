using Pulse.Api.Contracts.Responses;
using Pulse.Application.Pagination;
using Pulse.Domain.Models;

namespace Pulse.Api.Mapping;

public static class PageMapping
{
    public static PagedResponse<TaskResponse> ToPagedResponse(this PageResult<TaskItem> page) => new(
        Page: page.Page,
        Size: page.Size,
        Total: page.Total,
        Items: page.Items.Select(i => i.ToResponse()).ToArray()
    );
}