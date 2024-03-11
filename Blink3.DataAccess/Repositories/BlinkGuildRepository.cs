using Blink3.Common.Caching;
using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;
namespace Blink3.DataAccess.Repositories;

public class BlinkGuildRepository(BlinkDbContext dbContext, ICachingService cache)
    : GenericRepositoryWithCaching<BlinkGuild>(dbContext, cache), IBlinkGuildRepository
{
    public async Task<BlinkGuild> GetOrCreateByIdAsync(params object[] keyValues)
    {
        ulong id = VerifyAndExtractGuildId(keyValues);
        
        BlinkGuild? guild = await GetByIdAsync(keyValues);
        if (guild != null) return guild;
        
        guild = new BlinkGuild
        {
            Id = id
        };
        
        await AddAsync(guild);
        return guild;
    }

    /// <summary>
    /// Verifies and extracts the Guild ID from the given key values.
    /// </summary>
    /// <param name="keyValues">The key values.</param>
    /// <returns>The extracted Guild ID.</returns>
    private static ulong VerifyAndExtractGuildId(object[] keyValues)
    {
        if (keyValues.Length < 1 || ulong.TryParse(keyValues[0].ToString(), out ulong id) != true)
        {
            throw new ArgumentOutOfRangeException(nameof(keyValues), "Invalid Guild ID");
        }
        
        return id;
    }
}