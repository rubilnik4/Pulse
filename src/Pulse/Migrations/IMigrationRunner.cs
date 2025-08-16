namespace Pulse.Migrations;

public interface IMigrationRunner
{
    Task RunAsync();
}