using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating BlinkUser entities.
/// </summary>
public interface IBlinkUserRepository : IGenericRepository<BlinkUser>
{
    /// <summary>
    ///     Retrieves the leaderboard of BlinkUsers.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable collection
    ///     of BlinkUser objects representing the leaderboard.
    /// </returns>
    public Task<IEnumerable<BlinkUser>> GetLeaderboardAsync();
}