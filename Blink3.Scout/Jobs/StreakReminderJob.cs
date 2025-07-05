using Blink3.Core.Entities;
using Blink3.Core.Helpers;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Rest;

namespace Blink3.Scout.Jobs;

// ReSharper disable once ClassNeverInstantiated.Global
public class StreakReminderJob(IServiceScopeFactory scopeFactory, ILogger<StreakResetJob> logger) : BaseStreakJob(scopeFactory, logger)
{
    public async Task ExecuteAsync()
    {
        using IServiceScope scope = ScopeFactory.CreateScope();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        DiscordRestClient client = scope.ServiceProvider.GetRequiredService<DiscordRestClient>();
        IReadOnlyCollection<GameStatistics> gameStats = await GetGameStatisticsAsync(scope);
        DateTime now = DateTime.UtcNow;

        logger.LogInformation("Processing streak reminders for {TotalUsers} users at {CurrentTime}", gameStats.Count, now);
        
        foreach (GameStatistics gameStat in gameStats)
        {
            if (StreakHelpers.ShouldSendReminder(gameStat, now))
            {
                await HandleStreakReminderAsync(client, gameStat, unitOfWork, now);
            }
        }
        
        await unitOfWork.SaveChangesAsync();
    }
    
    private async Task HandleStreakReminderAsync(IDiscordClient client, GameStatistics gameStat, IUnitOfWork unitOfWork, DateTime now)
    {
        IUser? user = await FetchUserDetailsAsync(client, gameStat.BlinkUserId);
        if (user == null)
        {
            Logger.LogWarning("Skipping user {BlinkUserId} due to missing user details.", gameStat.BlinkUserId);
            return;
        }

        string gameName = gameStat.Type.ToString();
        DateTime streakExpiry = StreakHelpers.GetStreakExpiry(gameStat);

        Logger.LogInformation("Sending streak reminder to {User}. Streak expiry: {ExpirationDate}",
            new UserLogContext(user), streakExpiry);

        TimestampTag expires = TimestampTag.FromDateTime(streakExpiry, TimestampTagStyles.Relative);
        await user.SendMessageAsync($"Hi {user.Mention}, your {gameName} streak of {gameStat.CurrentStreak} days is expiring {expires}. Keep it up!");

        gameStat.ReminderSentAt = now;
        await unitOfWork.GameStatisticsRepository.UpdateAsync(gameStat);
    }
}