using System.ComponentModel.DataAnnotations;

namespace Pulse.Api.Contracts.Requests;

public sealed class PagingQuery
{
    [Range(1, 1000)]
    public int Page { get; init; }

    [Range(1, 100)]
    public int Size { get; init; }
    
    [EnumDataType(typeof(TaskStatusDto))]
    public TaskStatusDto? Status { get; init; }
   
    [EnumDataType(typeof(TaskSortDto))]
    public TaskSortDto? Sort { get; init; }
}