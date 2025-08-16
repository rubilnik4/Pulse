using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Npgsql;

namespace Test.Pulse.Integrations.Startup;

public sealed class CustomWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // удалить зарегистрированный в Program.cs NpgsqlDataSource
            var toRemove = services.SingleOrDefault(d => d.ServiceType.Name == "NpgsqlDataSource");
            if (toRemove is not null) services.Remove(toRemove);

            var dsb = new NpgsqlDataSourceBuilder(connectionString);
            dsb.MapEnum<PulseTaskStatus>("task_status"); // маппинг PG enum ↔ C# enum
            var dataSource = dsb.Build();

            services.AddSingleton(dataSource);
        });
    }
}