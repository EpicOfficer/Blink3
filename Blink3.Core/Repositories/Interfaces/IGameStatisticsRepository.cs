using Blink3.Core.Entities;
using Blink3.Core.Enums;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Provides functionality for managing GameStatistics entities, including retrieving, creating, and updating
///     statistics.
/// </summary>
public interface IGameStatisticsRepository : IGenericRepository<GameStatistics>
{
    /// <summary>
    ///     Asynchronously retrieves the global game statistics associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom to retrieve global statistics.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the <see cref="GameStatistics" />
    ///     object representing the user's global statistics.
    /// </returns>
    public Task<GameStatistics> GetGlobalStatisticsAsync(ulong userId);

    /// <summary>
    ///     Retrieves existing game statistics for the specified user and game type or creates new statistics if none exist.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the game statistics are being retrieved or created.</param>
    /// <param name="gameType">The type of game for which the statistics are being retrieved or created.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the <see cref="GameStatistics" />
    ///     object for the specified user and game type.
    /// </returns>
    public Task<GameStatistics> GetOrCreateGameStatistics(ulong userId, GameType gameType);

    /// <summary>
    ///     Asynchronously retrieves the leaderboard, optionally filtered by game type.
    /// </summary>
    /// <param name="gameType">
    ///     An optional parameter to specify the game type for filtering the leaderboard.
    ///     If null, the leaderboard for all game types will be returned.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task's result contains an enumerable collection of
    ///     <see cref="GameStatistics" /> representing the leaderboard.
    /// </returns>
    public Task<IEnumerable<GameStatistics>> GetLeaderboardAsync(GameType? gameType = null);
}