using Npgsql;
using Testcontainers.PostgreSql;

namespace Test.Pulse.Integrations.Startup;

[SetUpFixture]
public sealed class TestDatabase
{
    public static PostgreSqlContainer Container { get; private set; } = default!;
    
    public static string ConnectionString => Container.GetConnectionString();

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase($"taskpulse_test_{Guid.NewGuid():N}")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        await Container.StartAsync();
        
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string sql = @"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'task_status') THEN
        CREATE TYPE task_status AS ENUM ('new', 'in_progress', 'completed', 'overdue');
    END IF;
END$$;

CREATE TABLE IF NOT EXISTS tasks (
    id              uuid PRIMARY KEY,
    title           text NOT NULL,
    description     text NULL,
    due_date_utc    timestamptz NOT NULL,
    status          task_status NOT NULL DEFAULT 'new',
    created_at_utc  timestamptz NOT NULL,
    updated_at_utc  timestamptz NOT NULL
);";

        await conn.ExecuteAsync(sql);
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await Container.DisposeAsync();
    }
}