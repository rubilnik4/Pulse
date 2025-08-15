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

builder.Services.AddSingleton<IDateTimeService, SystemDateTimeService>();

var cs = builder.Configuration.GetConnectionString("Postgres")
         ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

var dsb = new NpgsqlDataSourceBuilder(cs);
dsb.MapEnum<PulseTaskStatus>("task_status");
var dataSource = dsb.Build();
builder.Services.AddSingleton(dataSource);

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddHostedService<OverdueTasksWorker>(); // реализуй IHostedService/BackgroundService

var app = builder.Build();

// ---------- OpenAPI UI ----------
app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(); // для AddOpenApi
    // если используешь AddSwaggerGen(), тогда: app.UseSwagger(); app.UseSwaggerUI();
}

// ---------- Глобальные фильтры/конвейер (по желанию) ----------
// app.UseHttpsRedirection();

// ---------- Endpoints ----------
app.MapGroup("/api")              // можно повесить общие фильтры/политики на группу
   .WithTags("API")
   .MapGroup("/tasks")            // либо вызываем вашу экстеншен-функцию
   .WithTags("Tasks");

// Если у тебя есть extension вида: public static RouteGroupBuilder GetEndpoints(this IEndpointRouteBuilder app)
TasksEndpoints.GetEndpoints(app); // ← подключаем ваши task-эндпоинты (Create/List/Get/Update/Patch/Delete)

// (Опционально) эндпоинт курсов валют, если реализован
// app.MapGroup("/api/rates").MapGet("/", CurrencyHandlers.GetLatest);

app.Run();

