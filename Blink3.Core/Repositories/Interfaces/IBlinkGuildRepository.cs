using Blink3.Core.Entities;

// ReSharper disable UnusedMember.Global

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating BlinkGuild entities.
/// </summary>
public interface IBlinkGuildRepository : IGenericRepository<BlinkGuild>
{
    /// <summary>
    ///     Finds and retrieves a collection of BlinkGuild entities with the specified IDs asynchronously.
    /// </summary>
    /// <param name="ids">The IDs of the BlinkGuild entities to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a read-only collection of BlinkGuild entities.</returns>
    public Task<IReadOnlyCollection<BlinkGuild>> FindByIdsAsync(HashSet<ulong> ids);
}