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
    /// <returns>A list of downloaded file attachments.</returns>
    Task<IDisposableCollection<FileAttachment>> DownloadAttachmentsFromMessageAsync(IMessage message);
}