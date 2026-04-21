using CaseyHub.API.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CaseyHub.API.Workers;

public class PermitNightlySyncWorker(
    IServiceProvider serviceProvider,
    ILogger<PermitNightlySyncWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PermitNightlySyncWorker started. Calculating initial schedule...");

        TimeZoneInfo targetTimeZone;
        try
        {
            targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Melbourne");
        }
        catch (TimeZoneNotFoundException)
        {
            targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var localTime = TimeZoneInfo.ConvertTime(now, targetTimeZone);

            var targetLocalTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, 2, 0, 0, DateTimeKind.Unspecified);
            var targetTimeOffset = new DateTimeOffset(targetLocalTime, targetTimeZone.GetUtcOffset(targetLocalTime));

            // If 2:00 AM has already passed today, move the target to 2:00 AM tomorrow
            if (now > targetTimeOffset)
            {
                targetLocalTime = targetLocalTime.AddDays(1);
                targetTimeOffset = new DateTimeOffset(targetLocalTime, targetTimeZone.GetUtcOffset(targetLocalTime));
            }

            var delay = targetTimeOffset - now;
            logger.LogInformation("Next Permit Sync scheduled for {TargetTime} (in {Hours}h {Minutes}m {Seconds}s).", 
                targetTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz"), delay.Hours, delay.Minutes, delay.Seconds);

            // Suspend the background thread until the exact scheduled time
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Triggering Nightly Permit Sync...");
                await TriggerSyncAsync();
            }
        }
    }

    private async Task TriggerSyncAsync()
    {
        //BackgroundService is a Singleton. IPermitService is Scoped.
        //Creating an isolated scope for the duration of the sync.
        using var scope = serviceProvider.CreateScope();
        var permitService = scope.ServiceProvider.GetRequiredService<IPermitService>();

        try
        {
            await permitService.SyncPermitsAsync();
            logger.LogInformation("Nightly Permit Sync completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nightly Permit Sync failed during execution.");
        }
    }
}