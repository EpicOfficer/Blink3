using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Helpers;

public static class StreakHelper
{
    public static void UpdateStreak(GameStatistics userStats, DateTime currentTime)
    {
        bool isConsecutiveDay = userStats.LastActivity?.Date.AddDays(1) == currentTime.Date;

        if (isConsecutiveDay)
        {
            userStats.CurrentStreak++;
            userStats.MaxStreak = Math.Max(userStats.MaxStreak, userStats.CurrentStreak);
        }
        else
        {
            userStats.MaxStreak = Math.Max(userStats.MaxStreak, userStats.CurrentStreak);
            userStats.CurrentStreak = 0;
        }
    }
    
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
}