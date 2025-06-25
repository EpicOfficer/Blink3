using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
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
    private const int DaysInactiveThreshold = 2; // Threshold for inactivity in days
    private const int ReminderThresholdHours = 1; // Threshold in hours for streak reminders
    private const int FlexibleMinutes = 30; // Flexible range for streak reminders in minutes
    private const int TimerInterval = 2; // Interval in hours for the timer to execute
    private readonly ILogger<DiscordClientService> _logger = logger;
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
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IReadOnlyCollection<GameStatistics> gameStats = await unitOfWork.GameStatisticsRepository.GetAllAsync();
        DateTime now = DateTime.UtcNow;

        _logger.LogDebug("Processing streak resets for {TotalUsers} users at {CurrentTime}", gameStats.Count, now);
        
        foreach (GameStatistics gameStat in gameStats)
        {
            await SendStreakReminderAsync(gameStat, unitOfWork, now);
            await ResetUserStreakAsync(gameStat, unitOfWork, now);
        }
    }

    private async Task SendStreakReminderAsync(GameStatistics gameStat, IUnitOfWork unitOfWork, DateTime now)
    {
        if (gameStat is { LastActivity: not null, CurrentStreak: > 0 })
        {
            DateTime streakExpiry = gameStat.LastActivity.Value.AddDays(DaysInactiveThreshold);
            TimeSpan timeToExpiry = streakExpiry - now;

            // Use a flexible range for reminders (-30 mins to +30 mins of the threshold)
            TimeSpan minThreshold = TimeSpan.FromHours(ReminderThresholdHours) - TimeSpan.FromMinutes(FlexibleMinutes);
            TimeSpan maxThreshold = TimeSpan.FromHours(ReminderThresholdHours) + TimeSpan.FromMinutes(FlexibleMinutes);
            
            // Skip if a reminder has been sent recently (inside the max flexible range)
            if (gameStat.ReminderSentAt.HasValue && gameStat.ReminderSentAt.Value >= streakExpiry - maxThreshold)
            {
                _logger.LogDebug("Skipping reminder for user {BlinkUserId}, reminder already sent in the allowed range.", gameStat.BlinkUserId);
                return;
            }

            // Skip if the time to expiry is outside the allowed range
            if (timeToExpiry > maxThreshold || timeToExpiry < minThreshold)
            {
                _logger.LogDebug("Skipping reminder for user {BlinkUserId}, time to expiry is outside the allowed range.", gameStat.BlinkUserId);
                return;
            }

            IUser user = await FetchUserDetailsAsync(gameStat.BlinkUserId);
            UserLogContext userContext = new(user);

            _logger.LogInformation(
                "Sending streak reminder to {UserContext}. Time to expiry: {TimeToExpiry}",
                userContext, timeToExpiry);

            // Send DM reminder to the user
            string gameName = Enum.GetName(gameStat.Type) ?? "Wordle";
            await SendUserStreakReminderAsync(user, gameName, streakExpiry);

            // Mark reminder as sent
            gameStat.ReminderSentAt = now;
            await unitOfWork.GameStatisticsRepository.UpdateAsync(gameStat);
            await unitOfWork.SaveChangesAsync();
        }
    }
    
    private async Task SendUserStreakReminderAsync(IUser user, string gameName, DateTime streakExpiry)
    {
        UserLogContext userContext = new(user);
        
        try
        {
            string reminderMessage = $"Hi {userContext.UserName}, your {gameName} streak is about to expire on {new TimestampTag(streakExpiry, TimestampTagStyles.Relative)}.\n" +
                                     "Make sure to extend your streak before then!";
            
            await user.SendMessageAsync(reminderMessage);

            _logger.LogInformation("Streak reminder sent to {UserContext}.", userContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send streak reminder to {UserContext}.", userContext);
        }
    }
    
    /// <summary>
    ///     Resets a user's streak based on inactivity and updates their record.
    /// </summary>
    private async Task ResetUserStreakAsync(GameStatistics gameStat, IUnitOfWork unitOfWork, DateTime now)
    {
        if (gameStat.CurrentStreak <= 0 || gameStat.LastActivity?.Date.AddDays(DaysInactiveThreshold) > now) return;

        UserLogContext userContext = new(await FetchUserDetailsAsync(gameStat.BlinkUserId));
        _logger.LogInformation("Resetting streak for {userContext}...", userContext);
        gameStat.MaxStreak = Math.Max(gameStat.MaxStreak, gameStat.CurrentStreak);
        gameStat.CurrentStreak = 0;

        // Clear the reminder flag after streak reset
        gameStat.ReminderSentAt = null;
        
        await unitOfWork.GameStatisticsRepository.UpdateAsync(gameStat);
        await unitOfWork.SaveChangesAsync();
    }
    
    private async Task<IUser> FetchUserDetailsAsync(ulong userId)
    {
        IUser? user = await Client.GetUserAsync(userId);
        if (user != null) return user;
        
        _logger.LogWarning("Unable to fetch Discord user details for userId {UserId}.", userId);
        throw new Exception($"User with ID {userId} not found.");
    }
}