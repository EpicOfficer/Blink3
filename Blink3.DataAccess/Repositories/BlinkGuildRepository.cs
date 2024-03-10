using Blink3.DataAccess.Entities;
using Blink3.DataAccess.Interfaces;

namespace Blink3.DataAccess.Repositories;

public class BlinkGuildRepository(BlinkDbContext dbContext) : GenericRepository<BlinkGuild>(dbContext), IBlinkGuildRepository
{
    public async Task<BlinkGuild> GetOrCreateByIdAsync(params object[] keyValues)
    {
        if (keyValues.Length < 1 ||
            ulong.TryParse(keyValues[0].ToString(), out ulong id) != true)
        {
            throw new ArgumentOutOfRangeException(nameof(keyValues), "Invalid Guild ID");
        }
        
        BlinkGuild? guild = await base.GetByIdAsync(keyValues);
        if (guild != null) return guild;
        
        guild = new BlinkGuild
        {
            Id = id
        };

        await AddAsync(guild);
        return guild;
    }
}