using Blink3.Core.Caching;
using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkGuildRepository" />
public class BlinkGuildRepository(BlinkDbContext dbContext, ICachingService cache)
    : GenericRepositoryWithCaching<BlinkGuild>(dbContext, cache), IBlinkGuildRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<BlinkGuild>> FindByIdsAsync(HashSet<ulong> ids)
    {
        return await _dbContext.BlinkGuilds.Where(bg => ids.Contains(bg.Id)).ToListAsync();
    }
}