using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class GameStatisticsRepository(BlinkDbContext dbContext)
    : GenericRepository<GameStatistics>(dbContext), IGameStatisticsRepository
{
    private const int LeaderboardSize = 8;
    private readonly BlinkDbContext _dbContext = dbContext;

    public async Task<GameStatistics> GetGlobalStatisticsAsync(ulong userId)
    {
        GameStatistics? globalStatistics = await _dbContext.GameStatistics
            .Where(x => x.BlinkUserId == userId) // Filter by user ID
            .GroupBy(x => x.BlinkUserId)        // Group by user ID
            .Select(group => new GameStatistics
            {
                BlinkUserId = userId,
                GamesPlayed = group.Sum(x => x.GamesPlayed),          // Sum of total games played
                GamesWon = group.Sum(x => x.GamesWon),                // Sum of total games won
                Points = group.Sum(x => x.Points),                    // Sum of total points
                CurrentStreak = group.Max(x => x.CurrentStreak),      // Take the maximum current streak
                MaxStreak = group.Max(x => x.MaxStreak),              // Take the maximum streak
                LastActivity = group.Max(x => x.LastActivity)         // Take the most recent activity
            })
            .FirstOrDefaultAsync(); // Get the global statistics (or null if no stats exist)

        return globalStatistics ?? new GameStatistics
        {
            BlinkUserId = userId,
            GamesPlayed = 0,
            GamesWon = 0,
            Points = 0,
            CurrentStreak = 0,
            MaxStreak = 0,
            LastActivity = null
        };
    }
    
    public async Task<GameStatistics> GetOrCreateGameStatistics(ulong userId, GameType gameType)
    {
        // Attempt to retrieve the GameStatistics
        GameStatistics? stats = await _dbContext.GameStatistics
            .FirstOrDefaultAsync(x => x.BlinkUserId == userId && x.Type == gameType);

        if (stats != null) return stats;
        
        BlinkUser? blinkUser = await _dbContext.BlinkUsers.FindAsync(userId);
        if (blinkUser == null)
        {
            blinkUser = new BlinkUser
            {
                Id = userId
            };

            _dbContext.BlinkUsers.Add(blinkUser);
            // Save for generated IDs or constraints (like automating other FK relationships)
            await _dbContext.SaveChangesAsync(); 
        }
        
        stats = new GameStatistics
        {
            BlinkUserId = userId,
            GamesPlayed = 0,
            GamesWon = 0,
            Points = 0,
            CurrentStreak = 0,
            MaxStreak = 0,
            Type = gameType
        };

        // Add the new entity to the DbSet (start tracking it)
        _dbContext.GameStatistics.Add(stats);

        // Save changes immediately for new entities to generate their Id
        await _dbContext.SaveChangesAsync();

        return stats;
    }

    public async Task<IEnumerable<GameStatistics>> GetLeaderboardAsync(GameType? gameType = null)
    {
        IQueryable<GameStatistics> query = _dbContext.GameStatistics.AsNoTracking();

        if (gameType.HasValue) query = query.Where(g => g.Type == gameType);

        List<GameStatistics> leaderboard = await query
            .Where(g => g.Points > 0) // Only include users with points > 0
            .GroupBy(g => g.BlinkUserId)
            .Select(group => new GameStatistics
            {
                BlinkUserId = group.Key,
                Points = group.Sum(g => g.Points),                // Total points across all game types
                GamesPlayed = group.Sum(g => g.GamesPlayed),      // Total of games played
                GamesWon = group.Sum(g => g.GamesWon),            // Total of games won
                CurrentStreak = group.Max(g => g.CurrentStreak),  // Max current streak
                MaxStreak = group.Max(g => g.MaxStreak),          // Max streak
                LastActivity = group.Max(g => g.LastActivity)     // Most recent activity
            })
            .OrderByDescending(stats => stats.Points) // Order by aggregated points
            .Take(LeaderboardSize) // Fetch only the top records (global leaderboard size)
            .ToListAsync();

        return leaderboard;
    }
}