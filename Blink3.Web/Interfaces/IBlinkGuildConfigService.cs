using Blink3.Core.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Blink3.Web.Interfaces;

/// <summary>
///     Represents a service for interacting with BlinkGuild configuration.
/// </summary>
public interface IBlinkGuildConfigService
{
    /// <summary>
    ///     Retrieves a BlinkGuild object from the API by its ID.
    /// </summary>
    /// <param name="id">The ID of the BlinkGuild to retrieve.</param>
    /// <returns>The retrieved BlinkGuild object, or null if not found.</returns>
    public Task<BlinkGuild?> GetByIdAsync(string? id);

    /// <summary>
    ///     Asynchronously applies a JSON patch to update a BlinkGuild entity.
    /// </summary>
    /// <param name="id">The ID of the BlinkGuild entity to be updated.</param>
    /// <param name="patchDocument">The JSON patch document containing the updates.</param>
    /// <returns>
    ///     A boolean value indicating whether the patch operation was successful.
    /// </returns>
    public Task<bool> PatchAsync(string? id, JsonPatchDocument<BlinkGuild> patchDocument);
}