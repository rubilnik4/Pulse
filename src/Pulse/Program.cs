using Microsoft.Extensions.Options;
using Pulse.Api.Endpoints;
using Pulse.Application.Options;
using Pulse.Application.Services;
using Pulse.Application.Services.Cbr;
using Pulse.Infrastructure.Database;
using Pulse.Infrastructure.Repositories;
using Pulse.Infrastructure.Telemetry;
using Pulse.Infrastructure.Workers;
using Pulse.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.AddOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.AddTelemetry();

builder.Services.AddMemoryCache();

var postgresOptions = builder.Configuration.GetSection("Postgres").Get<PostgresOptions>()
                      ?? throw new InvalidOperationException("Postgres options not bound");
var dataSource = DataSourceFactory.Create(postgresOptions.BuildConnectionString());
builder.Services.AddSingleton<IMigrationRunner, PostgresMigrationRunner>();
builder.Services.AddSingleton(dataSource);

builder.Services.AddSingleton<IDateTimeService, SystemDateTimeService>();
builder.Services.AddSingleton<IRateService, RateService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddHttpClient<ICbrClient, CbrClient>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<CbrOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
});

builder.Services.AddHostedService<OverdueTasksWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    await migrator.RunAsync();
}

app.MapOpenApi();
app.UseSwaggerUI();

app.GetTasksEndpoints();
app.GetRatesEndpoints();

app.Run();

