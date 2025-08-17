using Microsoft.Extensions.Options;
using Pulse.Application.Options;
using Pulse.Application.Services;
using Pulse.Infrastructure.Repositories;

namespace Pulse.Infrastructure.Workers;

public sealed class OverdueTasksWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<OverdueWorkerOptions> options,
    ILogger<OverdueTasksWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            log.LogInformation("OverdueTasksWorker disabled via configuration.");
            return;
        }

        var period = TimeSpan.FromSeconds(Math.Max(5, options.Value.PeriodSeconds));
        log.LogInformation("OverdueTasksWorker started, period = {Period}", period);
        
        using var timer = new PeriodicTimer(period);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ITaskService>();
               
                await service.MarkOverdue();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                log.LogError(ex, "Overdue sweep tick crashed: {Message}", ex.Message);
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException) { break; }
        }

        log.LogInformation("OverdueTasksWorker stopped");
    }
}