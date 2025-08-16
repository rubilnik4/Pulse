using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Application.Services;
using Pulse.Infrastructure.Database;
using Testcontainers.PostgreSql;
using Program = Pulse.Program;

namespace Test.Pulse.Integrations.Startup;

public sealed class CustomWebApplicationFactory() : WebApplicationFactory<Program>
{
    private PostgreSqlContainer? _postgres;
    
    public FakeDateTimeService DateTimeService { get; } = new
        (new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    
    private async Task StartAsync()
    {
        if (_postgres != null) return;

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase($"taskpulse_test_{Guid.NewGuid():N}")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        await _postgres.StartAsync();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            if (string.IsNullOrWhiteSpace(_postgres?.GetConnectionString()))
                throw new InvalidOperationException("Factory not started. Call StartAsync() before CreateClient().");
            
            var toRemove = services.SingleOrDefault(d => d.ServiceType.Name == "NpgsqlDataSource");
            if (toRemove is not null) services.Remove(toRemove);

            var dataSource = DataSourceFactory.Create(_postgres.GetConnectionString());
            services.AddSingleton(dataSource);
            
            var timeDesc = services.FirstOrDefault(d => d.ServiceType == typeof(IDateTimeService));
            if (timeDesc is not null) services.Remove(timeDesc);
            services.AddSingleton<IDateTimeService>(DateTimeService);
        });
    }
    
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }
    
    public static async Task<CustomWebApplicationFactory> StartNewAsync()
    {
        var f = new CustomWebApplicationFactory();
        await f.StartAsync();
        return f;
    }
}