using System.ComponentModel.DataAnnotations;

namespace Pulse.Application.Options;

public sealed class OverdueWorkerOptions
{
    public required bool Enabled { get; init; } = true;
    
    [Range(5, 3600)]
    public required int PeriodSeconds { get; init; } = 60;
}