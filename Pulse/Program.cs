using Npgsql;
using Pulse.Api.Endpoints;
using Pulse.Application.Options;
using Pulse.Application.Services;
using Pulse.Domain.Models;
using Pulse.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection("Pagination"))
    .ValidateDataAnnotations()
    .Validate(o => o.DefaultSize <= o.MaxSize, "DefaultSize must be <= MaxSize")
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

var postgresOptions = builder.Configuration.GetSection("Postgres").Get<PostgresOptions>()
                      ?? throw new InvalidOperationException("Postgres options not bound");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(postgresOptions.BuildConnectionString());
dataSourceBuilder.MapEnum<PulseTaskStatus>("task_status");
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

builder.Services.AddSingleton<IDateTimeService, SystemDateTimeService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

//builder.Services.AddHostedService<OverdueTasksWorker>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI();

app.GetEndpoints();

// (Опционально) эндпоинт курсов валют, если реализован
// app.MapGroup("/api/rates").MapGet("/", CurrencyHandlers.GetLatest);

app.Run();

