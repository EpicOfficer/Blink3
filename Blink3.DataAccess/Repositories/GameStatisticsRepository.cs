using System.Diagnostics;
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
        GameStatistics? entity = _dbContext.Set<GameStatistics>().FirstOrDefault(g => g.BlinkUserId == userId && g.Type == gameType);
        if (entity is not null) return entity;
        Debug.Assert(entity != null, nameof(entity) + " != null");
        
        _dbContext.Entry(entity).State = EntityState.Detached;

        await AddAsync(entity).ConfigureAwait(false);

        return entity;
    }
}