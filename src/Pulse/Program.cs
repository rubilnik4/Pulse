using Pulse.Api.Endpoints;
using Pulse.Application.Options;
using Pulse.Application.Services;
using Pulse.Infrastructure.Database;
using Pulse.Infrastructure.Repositories;
using Pulse.Infrastructure.Telemetry;
using Pulse.Infrastructure.Workers;
using Pulse.Migrations;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services
    .AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection("Pagination"))
    .ValidateDataAnnotations()
    .Validate(o => o.DefaultSize <= o.MaxSize, "DefaultSize must be <= MaxSize")
    .ValidateOnStart();

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
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddHostedService<OverdueTasksWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrator = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    await migrator.RunAsync();
}

app.MapOpenApi();
app.UseSwaggerUI();

app.GetEndpoints();

// (Опционально) эндпоинт курсов валют, если реализован
// app.MapGroup("/api/rates").MapGet("/", CurrencyHandlers.GetLatest);

app.Run();

