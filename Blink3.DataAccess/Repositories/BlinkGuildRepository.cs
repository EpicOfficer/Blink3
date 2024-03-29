using Blink3.Common.Caching;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkGuildRepository" />
public class BlinkGuildRepository(BlinkDbContext dbContext, ICachingService cache)
    : GenericRepositoryWithCaching<BlinkGuild>(dbContext, cache), IBlinkGuildRepository
{
    public async Task<BlinkGuild> GetOrCreateByIdAsync(ulong id)
    {
        BlinkGuild? guild = await GetByIdAsync(id);
        if (guild is not null) return guild;

        guild = new BlinkGuild
        {
            Id = id
        };
        await AddAsync(guild);

        return guild;
    }
}