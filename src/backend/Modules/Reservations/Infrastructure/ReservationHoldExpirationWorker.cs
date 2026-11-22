using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TransportPlatform.Modules.Reservations.Application.Contracts;

namespace TransportPlatform.Modules.Reservations.Infrastructure;

public sealed class ReservationHoldExpirationWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<ReservationHoldExpirationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var expiredCount = await repository.ExpireHoldsAsync(timeProvider.GetUtcNow(), stoppingToken);
                if (expiredCount > 0)
                {
                    logger.LogInformation("Expired {ExpiredCount} reservation holds.", expiredCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to expire reservation holds; the next cycle will retry.");
            }
        }
    }
}
