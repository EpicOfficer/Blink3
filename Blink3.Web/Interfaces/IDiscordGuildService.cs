using Blink3.Core.Models;

namespace Blink3.Web.Interfaces;

/// <summary>
///     Represents a service for interacting with the Discord guild.
/// </summary>
public interface IDiscordGuildService
{
    /// <summary>
    ///     Retrieves the channels for a specific guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to retrieve the channels for.</param>
    /// <returns>
    ///     An asynchronous task that represents the operation. The task result contains a collection of
    ///     <see cref="DiscordPartialChannel" /> representing the channels of the specified guild.
    /// </returns>
    public Task<IEnumerable<DiscordPartialChannel>> GetChannels(string? guildId);

    /// <summary>
    ///     Retrieves the categories of channels within a specified guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild to retrieve the categories from.</param>
    /// <returns>
    ///     An enumerable collection of <see cref="DiscordPartialChannel" /> objects that represent the categories of channels.
    /// </returns>
    public Task<IEnumerable<DiscordPartialChannel>> GetCategories(string? guildId);
}