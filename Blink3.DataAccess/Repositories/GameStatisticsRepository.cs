using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class GameStatisticsRepository(BlinkDbContext dbContext)
    : GenericRepository<GameStatistics>(dbContext), IGameStatisticsRepository
{
    private const int LeaderboardSize = 10;
    private readonly BlinkDbContext _dbContext = dbContext;

    public async Task<GameStatistics> GetOrCreateGameStatistics(ulong userId, GameType gameType)
    {
        if (!_dbContext.BlinkUsers.Any(b => b.Id == userId))
        {
            await _dbContext.AddAsync(new BlinkUser
            {
                Id = userId
            });
        }
        
        GameStatistics? entity = await _dbContext.Set<GameStatistics>()
            .FirstOrDefaultAsync(g => g.BlinkUserId == userId && g.Type == gameType);
        if (entity is not null) return entity;

        entity = new GameStatistics
        {
            BlinkUserId = userId,
            Type = gameType
        };
        
        await AddAsync(entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<IEnumerable<GameStatistics>> GetLeaderboardAsync(GameType? gameType = null)
    {
        return gameType is null
            ? await _dbContext.GameStatistics
                .OrderByDescending(u => u.Points)
                .Take(LeaderboardSize)
                .ToArrayAsync()
            : await _dbContext.GameStatistics
                .Where(g => g.Type == gameType)
                .OrderByDescending(u => u.Points)
                .Take(LeaderboardSize)
                .ToArrayAsync();
    }
}