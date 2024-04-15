using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class TempVcRepository(BlinkDbContext dbContext) :
    GenericRepository<TempVc>(dbContext), ITempVcRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public async Task<TempVc?> GetByUserIdAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TempVcs.Where(u => u.GuildId == guildId && u.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}