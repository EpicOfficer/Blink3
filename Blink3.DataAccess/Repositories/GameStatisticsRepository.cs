using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkUserRepository" />
public class GameStatisticsRepository(BlinkDbContext dbContext)
    : GenericRepository<GameStatistics>(dbContext), IGameStatisticsRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;
    
    public async Task<GameStatistics> GetOrCreateGameStatistics(ulong userId, GameType gameType)
    {
        GameStatistics? stats =
            await _dbContext.GameStatistics.AsNoTracking()
                .FirstOrDefaultAsync(s => s.BlinkUserId == userId && s.Type == gameType);
        
        if (stats is not null) return stats;
        
        return await AddAsync(new GameStatistics
        {
            BlinkUserId = userId,
            Type = gameType
        });
    }
}