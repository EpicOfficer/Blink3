using Blink3.Core.DTOs;
using Blink3.Core.Entities;

namespace Blink3.Web.Interfaces;

/// <summary>
///     Represents an HTTP repository for accessing BlinkGuild entities.
/// </summary>
public interface IBlinkGuildHttpRepository
{
    /// <summary>
    ///     Retrieves all BlinkGuild entities.
    /// </summary>
    /// <returns>A Task that represents the asynchronous operation. The task result contains a collection of BlinkGuild entities.</returns>
    public Task<IEnumerable<BlinkGuild>> GetAsync();

    /// <summary>
    ///     Retrieves a BlinkGuild entity by ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the BlinkGuild entity to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of BlinkGuild entities.</returns>
    public Task<BlinkGuild> GetAsync(ulong id);

    /// <summary>
    /// Updates a BlinkGuild entity with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the BlinkGuild entity to update.</param>
    /// <param name="blinkGuild">The updated BlinkGuild entity.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task UpdateAsync(ulong id, BlinkGuild blinkGuild);
}