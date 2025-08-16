using Npgsql;
using Pulse.Application.Options;
using Pulse.Domain.Models;

namespace Pulse.Infrastructure.Database;

public static class DataSourceFactory
{
    public static NpgsqlDataSource Create(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.MapEnum<PulseTaskStatus>("task_status");
        return builder.Build();
    }
}