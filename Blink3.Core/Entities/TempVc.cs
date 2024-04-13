namespace Blink3.Core.Entities;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
/// <summary>
///     Represents a temporary voice channel.
/// </summary>
public class TempVc
{
    /// <summary>
    ///     Represents the unique identifier of a guild.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    ///     Represents the channel ID.
    /// </summary>
    public ulong ChannelId { get; set; }

    /// <summary>
    ///     Represents the ID of the user who created the temporary voice channel.
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    ///     Represents the time the temporary voice channel was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets a value indicating whether camera-only mode is enabled.
    /// </summary>
    /// <remarks>
    ///     If <see cref="CamOnly" /> is set to true, it means that users must have their camera enabled to remain in the
    ///     channel.
    /// </remarks>
    public bool CamOnly { get; set; }
}