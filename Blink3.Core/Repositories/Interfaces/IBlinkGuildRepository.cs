using Blink3.DataAccess.Entities;

namespace Blink3.DataAccess.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating BlinkGuild entities.
/// </summary>
public interface IBlinkGuildRepository : IGenericRepository<BlinkGuild>
{
    /// <summary>
    ///     Retrieves an entity by its key values asynchronously. If the entity is not found, creates a new entity with the
    ///     given key values.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains the entity if found or newly created,
    ///     otherwise null.
    /// </returns>
    Task<BlinkGuild> GetOrCreateByIdAsync(ulong id);
}