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
    private const int DaysInactiveThreshold = 2; // Threshold for inactivity in days
    private const int TimerInterval = 6; // Interval in hours for the timer to execute
    private Timer? _timer;

    /// <summary>
    ///     Executes the asynchronous operation for the StreakResetService when the service is starting.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        _logger.LogInformation("Streak reset service is starting...");
        _timer = new Timer(ExecuteStreakReset, null, TimeSpan.Zero, TimeSpan.FromHours(TimerInterval));
    }

    /// <summary>
    ///     Handles periodic execution for resetting user streaks.
    /// </summary>
    private void ExecuteStreakReset(object? state)
    {
        try
        {
            ProcessStreakResetsAsync().Forget();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred during streak reset execution.");
        }
    }

    /// <summary>
    ///     Processes streak resets asynchronously by iterating over user statistics.
    /// </summary>
    private async Task ProcessStreakResetsAsync()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IGameStatisticsRepository gameStatisticsRepository =
            scope.ServiceProvider.GetRequiredService<IGameStatisticsRepository>();
        IReadOnlyCollection<GameStatistics> gameStats = await gameStatisticsRepository.GetAllAsync();
        DateTime now = DateTime.UtcNow;

        foreach (GameStatistics gameStat in gameStats)
            await ResetUserStreakAsync(gameStat, gameStatisticsRepository, now);
    }

    /// <summary>
    ///     Resets a user's streak based on inactivity and updates their record.
    /// </summary>
    private async Task ResetUserStreakAsync(GameStatistics gameStat, IGameStatisticsRepository repository, DateTime now)
    {
        if (gameStat.CurrentStreak <= 0 || gameStat.LastActivity?.Date.AddDays(DaysInactiveThreshold) > now) return;

        _logger.LogInformation("Resetting streak for user {BlinkUserId}...", gameStat.BlinkUserId);
        gameStat.MaxStreak = Math.Max(gameStat.MaxStreak, gameStat.CurrentStreak);
        gameStat.CurrentStreak = 0;

        await repository.UpdateAsync(gameStat);
    }
}