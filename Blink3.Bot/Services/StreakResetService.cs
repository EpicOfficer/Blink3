using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Repositories.Interfaces;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Services;

public class StreakResetService(
    DiscordSocketClient client,
    ILogger<StreakResetService> logger,
    IServiceScopeFactory scopeFactory)
    : DiscordClientService(client, logger)
{
    private readonly ILogger<DiscordClientService> _logger = logger;
    private Timer? _timer;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        _logger.LogInformation("Streak reset service is starting...");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }
    
    private void DoWork(object? state)
    {
        try
        {
            DoWorkAsync().Forget();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured during streak reset timer");
        }
    }
    
    private async Task DoWorkAsync()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        DateTime time = DateTime.UtcNow;
        
        IGameStatisticsRepository gameStatisticsRepository = scope.ServiceProvider.GetRequiredService<IGameStatisticsRepository>();
        IReadOnlyCollection<GameStatistics> stats = await gameStatisticsRepository.GetAllAsync();
        foreach (GameStatistics stat in stats)
        {
            if (stat.LastActivity?.Date.AddDays(2) > time) continue;

            _logger.LogInformation("Resetting streak for {StatBlinkUserId}...", stat.BlinkUserId);

            // If it's been more than 1 day, reset the streak
            stat.MaxStreak = Math.Max(stat.MaxStreak, stat.CurrentStreak); // Update max streak before resetting
            stat.CurrentStreak = 0; // Reset current streak
            stat.LastActivity = time; // Update the last activity
            await gameStatisticsRepository.UpdateAsync(stat);
        }
    }
}