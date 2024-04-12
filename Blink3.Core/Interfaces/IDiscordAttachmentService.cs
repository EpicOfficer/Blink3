using Discord;

namespace Blink3.Core.Interfaces;

/// <summary>
///     Represents a service that handles downloading attachments from Discord messages.
/// </summary>
public interface IDiscordAttachmentService
{
    /// <summary>
    ///     Downloads the attachments from a message.
    /// </summary>
    /// <param name="message">The Discord message containing the attachments.</param>
    /// <param name="spoiler">Set the spoiler property of all retrieved attachments. Defaults to unmodified.</param>
    /// <returns>A list of downloaded file attachments.</returns>
    Task<IDisposableCollection<FileAttachment>> DownloadAsync(IMessage message, bool? spoiler = null);
}