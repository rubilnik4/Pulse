using System.ComponentModel.DataAnnotations;

namespace Pulse.Application.Options;

public sealed class CbrOptions
{
    [Required, Url]
    public required string BaseUrl { get; init; }
    
    [Required]
    public required string DailyPath { get; init; }
    
    [Range(1, 60)] 
    public required int CacheMinutes { get; init; } = 5;
}