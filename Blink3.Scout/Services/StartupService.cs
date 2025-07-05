using Blink3.Scout.Jobs;
using Hangfire;

namespace Blink3.Scout.Services;

public class StartupService(IRecurringJobManager jobManager) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        jobManager.AddOrUpdate<StreakReminderJob>(
            "blink-streak-reminder",
            job => job.ExecuteAsync(),
            Cron.Daily(22)
        );
        
        jobManager.AddOrUpdate<StreakResetJob>(
            "blink-streak-reset",
            job => job.ExecuteAsync(),
            Cron.Daily(0)
        );
        
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}