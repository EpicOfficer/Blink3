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
        GameStatistics globalStatistics = new()
        {
            BlinkUserId = userId
        };
        
        List<GameStatistics> allStats = await _dbContext.GameStatistics
            .Where(x => x.BlinkUserId == userId)
            .ToListAsync();
        
        globalStatistics = allStats.Aggregate(globalStatistics, (agg, stats) =>
        {
            agg.GamesPlayed += stats.GamesPlayed;   // Sum of total games played
            agg.GamesWon += stats.GamesWon;         // Sum of total games won
            agg.Points += stats.Points;             // Sum of total points
            agg.CurrentStreak = Math.Max(agg.CurrentStreak, stats.CurrentStreak); // Take the highest current streak
            agg.MaxStreak = Math.Max(agg.MaxStreak, stats.MaxStreak);             // Take the maximum streak
            return agg;
        });

        return globalStatistics;
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
        return gameType is null
            ? await _dbContext.GameStatistics
                .AsNoTracking()
                .Where(g => g.Points > 0)
                .OrderByDescending(u => u.Points)
                .Take(LeaderboardSize)
                .ToArrayAsync()
            : await _dbContext.GameStatistics
                .AsNoTracking()
                .Where(g => g.Type == gameType && g.Points > 0)
                .OrderByDescending(u => u.Points)
                .Take(LeaderboardSize)
                .ToArrayAsync();
    }
}