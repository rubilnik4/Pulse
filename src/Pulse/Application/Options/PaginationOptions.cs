using System.ComponentModel.DataAnnotations;
using Pulse.Domain.Models;

namespace Pulse.Application.Options;

public sealed class PaginationOptions
{
    [Range(1, 10000)] 
    public required int DefaultPage { get; init; }
    
    [Range(1, 10000)] 
    public required int DefaultSize { get; init; }
    
    [Range(1, 10000)] 
    public required int MaxSize { get; init; }
    
    public required PulseTaskSort DefaultSort { get; init; } = PulseTaskSort.DueDateAsc;
}