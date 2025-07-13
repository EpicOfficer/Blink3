using Blink3.Core.Base;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Blink3.DataAccess.Repositories;

public class BaseGameRepository<TGame>(BlinkDbContext dbContext): IBaseGameRepository<TGame> where TGame : GameBase
{
    public virtual async Task<TGame?> GetByIdAsync(params object[] keyValues)
    {
        return await dbContext.Set<TGame>().FindAsync(keyValues).ConfigureAwait(false);
    }
    
    public virtual async Task<TGame?> GetByChannelIdAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TGame>()
            .FirstOrDefaultAsync(w => w.ChannelId == channelId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TGame>().AnyAsync(w => w.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HashSet<GameStatistics>> GetOtherParticipantStatsAsync(TGame game, ulong userId,
        CancellationToken cancellationToken = default)
    {
        HashSet<ulong> players = new(game.Players);
        List<GameStatistics> stats = await dbContext.GameStatistics
            .AsNoTracking()
            .Where(s => players.Contains(s.BlinkUserId) &&
                        s.Type == GameType.BlinkWord &&
                        s.BlinkUserId != userId)
            .ToListAsync(cancellationToken);
        return [..stats];
    }
    
    public async Task<TGame> AddAsync(TGame entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TGame>().AddAsync(entity, cancellationToken).ConfigureAwait(false);

        return entity;
    }
    
    public TGame Update(TGame entity)
    {
        EntityEntry<TGame> entry = dbContext.Entry(entity);

        // Attach the entity if it is not being tracked
        if (entry.State == EntityState.Detached)
        {
            dbContext.Attach(entity);
        }
        
        dbContext.Entry(entity).State = EntityState.Modified;

        return entity;
    }
    
    public void Delete(TGame entity)
    {
        dbContext.Set<TGame>().Attach(entity);
        dbContext.Set<TGame>().Remove(entity);
    }
}