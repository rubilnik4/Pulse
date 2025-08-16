using Pulse.Application.Services;

namespace Pulse.Infrastructure.Repositories;

public sealed class SystemDateTimeService : IDateTimeService
{
    public DateTime UtcNow() => DateTime.UtcNow; // простой прокси к системным часам
}