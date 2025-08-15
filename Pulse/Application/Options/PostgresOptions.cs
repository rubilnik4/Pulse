using System.ComponentModel.DataAnnotations;

namespace Pulse.Application.Options;

public sealed class PostgresOptions
{
    [Required] public required string Host { get; init; }
    [Range(1, 65535)] public required int Port { get; init; } 
    [Required] public required string Database { get; init; } 
    [Required] public required string Username { get; init; }
    [Required] public required string Password { get; init; }

    public string BuildConnectionString() =>
        $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
}