using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class TempVcRepository(BlinkDbContext dbContext) :
    GenericRepository<TempVc>(dbContext), ITempVcRepository
{
    public async Task<TempVc?> GetByUserIdAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TempVcs.Where(u => u.GuildId == guildId && u.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}