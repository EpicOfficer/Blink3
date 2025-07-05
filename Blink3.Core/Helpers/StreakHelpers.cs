using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Helpers;

public static class StreakHelpers
{
    private const int DaysUntilNextStreak = 1;
    private const int StreakExpiryDays = 2;
    
    /// <summary>
    ///     Updates the user's streak based on their last activity and the current time.
    /// </summary>
    /// <param name="userStats">The user's game statistics to update.</param>
    /// <param name="currentTime">The current date and time to evaluate the streak.</param>
    private static void UpdateStreak(GameStatistics userStats, DateTime currentTime)
    {
        if (GetNextStreakDate(userStats) == currentTime.Date)
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
        GameStatistics stats = await unitOfWork.GameStatisticsRepository.GetOrCreateGameStatistics(userId, gameType);
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
    ///     Determines if the user's streak should be reset based on their last activity and the streak expiration period.
    /// </summary>
    /// <param name="gameStat">The user's game statistics containing information about the current streak and last activity.</param>
    /// <returns>True if the streak should be reset; otherwise, false.</returns>
    public static bool ShouldResetStreak(GameStatistics gameStat)
    {
        DateTime now = DateTime.UtcNow.Date;
        return gameStat is { CurrentStreak: > 0, LastActivity: not null } &&
               GetStreakExpiry(gameStat) <= now;
    }

    /// <summary>
    ///     Resets the user's streak and updates relevant fields.
    /// </summary>
    /// <param name="gameStat">The game statistics object containing streak information to be reset.</param>
    public static void ResetStreak(GameStatistics gameStat)
    {
        gameStat.MaxStreak = Math.Max(gameStat.MaxStreak, gameStat.CurrentStreak);
        gameStat.CurrentStreak = 0;
        gameStat.ReminderSentAt = null;
    }

    /// <summary>
    ///     Determines whether a streak reminder should be sent to the user based on their streak activity and reminder status.
    /// </summary>
    /// <param name="gameStat">The game statistics of the user, containing streak activity and reminder details.</param>
    /// <returns>True if a reminder should be sent, otherwise false.</returns>
    public static bool ShouldSendReminder(GameStatistics gameStat)
    {
        DateTime today = DateTime.UtcNow.Date;
        
        return gameStat is { CurrentStreak: > 0, LastActivity: not null } &&
               gameStat.ReminderSentAt?.Date != today &&
               GetNextStreakDate(gameStat) == today;
    }

    /// <summary>
    ///     Calculates the expiry date of the user's current streak based on their last activity.
    /// </summary>
    /// <param name="gameStat">The user's game statistics containing the last activity date.</param>
    /// <returns>The calculated expiry date for the streak, or the current date and time if the last activity is null.</returns>
    public static DateTime GetStreakExpiry(GameStatistics gameStat) =>
        gameStat.LastActivity?.Date.AddDays(StreakExpiryDays) ?? DateTime.UtcNow;

    /// <summary>
    ///     Calculates the next date when the user's streak is eligible for progression.
    /// </summary>
    /// <param name="gameStat">The game statistics of the user, containing the last activity date.</param>
    /// <returns>The next date the user's streak can be updated.</returns>
    public static DateTime GetNextStreakDate(GameStatistics gameStat) =>
        gameStat.LastActivity?.Date.AddDays(DaysUntilNextStreak) ?? DateTime.UtcNow.Date;
}