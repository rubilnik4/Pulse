namespace Pulse.Application.Options;

public sealed class OverdueWorkerOptions
{
    public bool Enabled { get; init; } = true;
   
    public int PeriodSeconds { get; init; } = 60;
}