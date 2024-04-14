using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating TempVc entities.
/// </summary>
public interface ITempVcRepository : IGenericRepository<TempVc>
{
    /// <summary>
    ///     Retrieves a TempVc entity by user id asynchronously.
    /// </summary>
    /// <param name="guildId">The guild id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a TempVc entity
    ///     or null if no entity is found for the specified user id in the specified guild.
    /// </returns>
    Task<TempVc?>
        GetByUserIdAsync(ulong guildId, ulong userId, CancellationToken cancellationToken = default);
}