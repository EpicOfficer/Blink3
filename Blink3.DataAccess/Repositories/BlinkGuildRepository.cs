using Blink3.Common.Entities;
using Blink3.Common.Interfaces;

namespace Blink3.DataAccess.Repositories;

public class BlinkGuildRepository(BlinkDbContext dbContext) : GenericRepository<BlinkGuild>(dbContext), IBlinkGuildRepository
{
    
}