using Blink3.DataAccess.Interfaces;
using Blink3.DataAccess.Entities;

namespace Blink3.DataAccess.Interfaces;

public interface IBlinkGuildRepository : IGenericRepository<BlinkGuild>
{
    Task<BlinkGuild> GetOrCreateByIdAsync(params object[] keyValues);
}