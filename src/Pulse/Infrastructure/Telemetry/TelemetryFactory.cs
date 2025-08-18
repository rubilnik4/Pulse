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
        var config = builder.Configuration.GetSection("OpenTelemetry");
        var otlpEndpoint = config.GetValue<string>("Endpoint") ?? throw new Exception("OpenTelemetry endpoint not found");
        var serviceName = config.GetValue<string>("ServiceName") ?? throw new Exception("OpenTelemetry service not found");
        
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            options.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            });
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                })
            )
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                }));

        return builder;
    }
}