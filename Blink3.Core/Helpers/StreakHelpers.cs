using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Helpers;

public static class StreakHelpers
{
    private const int DaysInactiveThreshold = 2; // Threshold for inactivity

    /// <summary>
    ///     Updates the user's streak based on their last activity and the current time.
    /// </summary>
    /// <param name="userStats">The user's game statistics to update.</param>
    /// <param name="currentTime">The current date and time to evaluate the streak.</param>
    private static void UpdateStreak(GameStatistics userStats, DateTime currentTime)
    {
        bool isConsecutiveDay = userStats.LastActivity?.Date.AddDays(1) == currentTime.Date;

        if (isConsecutiveDay)
        {
            userStats.CurrentStreak++;
        }
        else
        {
            userStats.CurrentStreak = 0;
        }
        
        userStats.MaxStreak = Math.Max(userStats.MaxStreak, userStats.CurrentStreak);
    }

    /// <summary>
    ///     Ensures that the given user's game statistics are updated based on their latest activity
    ///     for the specified game type, creating statistics if they do not already exist.
    /// </summary>
    /// <param name="unitOfWork">The unit of work providing access to repositories for retrieving and saving data.</param>
    /// <param name="userId">The unique identifier of the user whose statistics are being updated.</param>
    /// <param name="gameType">The type of game associated with the statistics to update or create.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated or newly created game statistics.</returns>
    public static async Task<GameStatistics> EnsureStatsUpdatedAsync(
        IUnitOfWork unitOfWork,
        ulong userId,
        GameType gameType
    )
    {
        // Retrieve current statistics or create them if missing
        GameStatistics stats =
            await unitOfWork.GameStatisticsRepository.GetOrCreateGameStatistics(userId, gameType);
        DateTime currentTime = DateTime.UtcNow;

        // Only update streak if the last activity was a different day
        if (stats.LastActivity?.Date != currentTime.Date)
        {
            UpdateStreak(stats, currentTime);
            stats.LastActivity = currentTime;
        }

        return stats;
    }
    
    /// <summary>
    ///     Determines if the user's streak should be reset based on inactivity.
    /// </summary>
    public static bool ShouldResetStreak(GameStatistics gameStat)
    {
        DateTime now = DateTime.UtcNow.Date;
        return gameStat is { CurrentStreak: > 0, LastActivity: not null } &&
               GetStreakExpiry(gameStat) <= now;
    }

    /// <summary>
    ///     Resets the user's streak and updates relevant fields.
    /// </summary>
    public static void ResetStreak(GameStatistics gameStat)
    {
        gameStat.MaxStreak = Math.Max(gameStat.MaxStreak, gameStat.CurrentStreak);
        gameStat.CurrentStreak = 0;
        gameStat.ReminderSentAt = null;
    }
    
    /// <summary>
    ///     Determines if a streak reminder should be sent based on the user's activity.
    /// </summary>
    public static bool ShouldSendReminder(GameStatistics gameStat)
    {
        if (gameStat.LastActivity == null || gameStat.CurrentStreak <= 0) return false;

        DateTime now = DateTime.UtcNow.Date;
        
        if (gameStat.ReminderSentAt?.Date == now ) return false;
        
        return gameStat.LastActivity.Value.Date.AddDays(1) == now;
    }

    /// <summary>
    ///     Calculates the expiry date of the user's current streak based on their last activity.
    /// </summary>
    /// <param name="gameStat">The user's game statistics containing the last activity date.</param>
    /// <returns>The calculated expiry date for the streak, or the current date and time if the last activity is null.</returns>
    public static DateTime GetStreakExpiry(GameStatistics gameStat)
    {
        return gameStat.LastActivity?.Date.AddDays(DaysInactiveThreshold) ?? DateTime.UtcNow;
    }
}