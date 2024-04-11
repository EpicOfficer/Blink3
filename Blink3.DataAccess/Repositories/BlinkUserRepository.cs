using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

/// <inheritdoc cref="IBlinkUserRepository" />
public class BlinkUserRepository(BlinkDbContext dbContext)
    : GenericRepository<BlinkUser>(dbContext), IBlinkUserRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;
    private const int LeaderboardSize = 5;
    
    public async Task<IEnumerable<BlinkUser>> GetLeaderboardAsync()
    {
        return await _dbContext.BlinkUsers
            .OrderByDescending(u => u.Points)
            .Take(LeaderboardSize)
            .ToArrayAsync();
    }
}