using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Npgsql;

namespace Pulse.Migrations;

public sealed class PostgresMigrationRunner(NpgsqlDataSource ds, ILogger<IMigrationRunner> log) : IMigrationRunner
{
    private readonly Assembly _assembly = typeof(PostgresMigrationRunner).Assembly;

    public async Task RunAsync()
    {
        await using var conn = await ds.OpenConnectionAsync();
       
        log.LogInformation("Checking migration table");
        const string ensureSql = @"
        CREATE TABLE IF NOT EXISTS app_migrations
        (
            version     integer      NOT NULL,
            name        text         NOT NULL,
            applied_at  timestamptz  NOT NULL DEFAULT now(),
            CONSTRAINT pk_app_migrations PRIMARY KEY (version)
        );";
        await conn.ExecuteAsync(new CommandDefinition(ensureSql));
        
        var applied = (await conn.QueryAsync<(int version, string name)>(
                "SELECT version, name FROM app_migrations ORDER BY version"))
            .ToDictionary(x => x.version, x => x.name);
        
        var resourcePrefix = typeof(PostgresMigrationRunner).Namespace + ".";
        var resources = _assembly
            .GetManifestResourceNames()
            .Where(n => n.StartsWith(resourcePrefix, StringComparison.Ordinal))
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(n => new
            {
                Name = n,
                File = n[resourcePrefix.Length..],
            })
            .Select(x =>
            {
                var parts = x.File.Split("__", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || !int.TryParse(parts[0], out var ver))
                    throw new InvalidOperationException($"Bad migration name '{x.File}'. Expected 'NNNN__name.sql'");
                return (Version: ver, File: x.File, ResourceName: x.Name);
            })
            .OrderBy(x => x.Version)
            .ToArray();

        foreach (var migration in resources)
        {
            await using var stream = _assembly.GetManifestResourceStream(migration.ResourceName)
                                     ?? throw new InvalidOperationException($"Migration resource not found: {migration.ResourceName}");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var sql = await reader.ReadToEndAsync();

            if (applied.TryGetValue(migration.Version, out var existing))
            {
                if (!string.Equals(existing, migration.File, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"Name mismatch for migration {migration.Version}. Expected {existing}, got {migration.File}.");
                log.LogDebug("Migration {Version} already applied, skip", migration.Version);
                continue;
            }

            log.LogInformation("Applying migration {Version}: {File}", migration.Version, migration.File);
           
            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                await conn.ExecuteAsync(new CommandDefinition(sql, transaction: transaction));

                const string ins = @"INSERT INTO app_migrations(version, name) VALUES (@v, @n);";
                await conn.ExecuteAsync(new CommandDefinition(ins,
                    new { v = migration.Version, n = migration.File }, transaction: transaction));

                await transaction.CommitAsync();
                log.LogInformation("Migration {Version} applied", migration.Version);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}