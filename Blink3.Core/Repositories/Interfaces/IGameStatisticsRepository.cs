using Blink3.Core.Entities;
using Blink3.Core.Enums;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating GameStatistics entities.
/// </summary>
public interface IGameStatisticsRepository : IGenericRepository<GameStatistics>
{
    public Task<GameStatistics> GetOrCreateGameStatistics(ulong userId, GameType gameType);

    public Task<IEnumerable<GameStatistics>> GetLeaderboardAsync(GameType? gameType = null);
}