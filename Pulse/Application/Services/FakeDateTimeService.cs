namespace Pulse.Application.Services;

public sealed class FakeDateTimeService(DateTime initialUtc) : IDateTimeService
{
    private DateTime _utc = DateTime.SpecifyKind(initialUtc, DateTimeKind.Utc);

    public DateTime UtcNow() => _utc;

    public void Advance(TimeSpan by) =>
        _utc = _utc.Add(by);
    
    public void Set(DateTime utc) => 
        _utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
}