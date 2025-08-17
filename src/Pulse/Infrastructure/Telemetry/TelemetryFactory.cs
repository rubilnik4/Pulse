using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Pulse.Infrastructure.Telemetry;

public static class TelemetryFactory
{
    public static WebApplicationBuilder AddTelemetry(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            //options.AddConsoleExporter();
        });
        
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("pulse"))
            .WithTracing(t => t
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddConsoleExporter()
            )
            .WithMetrics(m => m
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter());
        
        // builder.Services.AddOpenTelemetry()
        //     .ConfigureResource(res => res.AddService(
        //         serviceName: "taskpulse-api",
        //         serviceVersion: "1.0.0",
        //         serviceInstanceId: Environment.MachineName))
        //     .WithTracing(tracer =>
        //     {
        //         tracer
        //             .AddAspNetCoreInstrumentation(o =>
        //             {
        //                 o.RecordException = true;
        //                 o.Filter = ctx => true; // можно фильтровать ненужные запросы
        //             })
        //             .AddHttpClientInstrumentation()
        //             .AddNpgsql() // трассировка Npgsql (команды БД как спаны)
        //             // Экспортер спанов в консоль (читабельный):
        //             .AddConsoleExporter();
        //         // .AddOtlpExporter() // опц.: в OTLP-коллектор (Jaeger/Tempo/OTel-Collector)
        //     })
        //     .WithMetrics(meter =>
        //     {
        //         meter
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddRuntimeInstrumentation()
        //             .AddProcessInstrumentation()
        //             .AddConsoleExporter(); // метрики в консоль
        //     });
        return builder;
    }
}