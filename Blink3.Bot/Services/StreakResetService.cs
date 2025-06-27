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
    private static readonly TimeSpan ReminderWindowStartOffset = TimeSpan.FromHours(2);
    private static readonly TimeSpan ReminderWindowEndOffset = TimeSpan.FromMinutes(5);
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
        
        await unitOfWork.SaveChangesAsync();
    }

    private async Task SendStreakReminderAsync(GameStatistics gameStat, IUnitOfWork unitOfWork, DateTime now)
    {
        if (gameStat is { LastActivity: not null, CurrentStreak: > 0 })
        {
            DateTime streakExpiry = gameStat.LastActivity.Value.AddDays(DaysInactiveThreshold);
            TimeSpan timeToExpiry = streakExpiry - now;

            DateTime windowStart = streakExpiry - ReminderWindowStartOffset;
            DateTime windowEnd = streakExpiry - ReminderWindowEndOffset;

            // Skip if a reminder has already been sent for this expiry window
            if (gameStat.ReminderSentAt.HasValue && gameStat.ReminderSentAt.Value >= windowStart)
            {
                _logger.LogDebug("Skipping reminder for user {BlinkUserId}, reminder already sent during this window.", gameStat.BlinkUserId);
                return;
            }

            // Only send it if we're inside the window
            if (now < windowStart || now > windowEnd)
            {
                _logger.LogDebug(
                    "Skipping reminder for user {BlinkUserId}, outside reminder window. Now: {Now}, Window: {WindowStart} to {WindowEnd}",
                    gameStat.BlinkUserId, now, windowStart, windowEnd);
                return;
            }

            IUser? user = await FetchUserDetailsAsync(gameStat.BlinkUserId);
            if (user == null)
            {
                _logger.LogWarning("Skipping user {BlinkUserId} due to missing user details.", gameStat.BlinkUserId);
                return;
            }
            UserLogContext userContext = new(user);

            _logger.LogInformation(
                "Sending streak reminder to {UserContext}. Time to expiry: {TimeToExpiry}",
                userContext, timeToExpiry);

            // Send DM reminder to the user
            string gameName = Enum.GetName(gameStat.Type) ?? "Wordle";
            await SendUserStreakReminderAsync(user, gameName, gameStat.CurrentStreak, streakExpiry);

            // Mark reminder as sent
            gameStat.ReminderSentAt = now;
            await unitOfWork.GameStatisticsRepository.UpdateAsync(gameStat);
        }
    }
    
    private async Task SendUserStreakReminderAsync(IUser user, string gameName, int currentStreak, DateTime streakExpiry)
    {
        UserLogContext userContext = new(user);
        
        try
        {
            string reminderMessage = $"""
                                      Hi {user.Mention}! 👋

                                      Your **{gameName} streak** of **{currentStreak} days** is set to expire {new TimestampTag(streakExpiry, TimestampTagStyles.Relative)}. 🕒  

                                      It would be a shame to lose your **{currentStreak}-day streak**.  
                                      Don't miss out! Jump in now to keep it alive! 🎮💪
                                      """;
            
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
    }
    
    private async Task<IUser?> FetchUserDetailsAsync(ulong userId)
    {
        try
        {
            IUser? user = await Client.GetUserAsync(userId);
            if (user != null) return user;

            _logger.LogWarning("User with ID {UserId} could not be found.", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user details for userId {UserId}.", userId);
            return null;
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Streak reset service is stopping...");
        _timer?.Change(Timeout.Infinite, 0);
        return base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        base.Dispose();
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}