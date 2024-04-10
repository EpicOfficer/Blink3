using Blink3.Core.Helpers;
using Blink3.Core.Interfaces;
using Discord;

namespace Blink3.Bot.Services;

/// <inheritdoc />
public class DiscordAttachmentService(HttpClient httpClient) : IDiscordAttachmentService
{
    public async Task<IDisposableCollection<FileAttachment>> DownloadAttachmentsFromMessageAsync(IMessage message)
    {
        IEnumerable<Task<FileAttachment>> downloadTasks = message.Attachments.Select(
            async attachment => await CreateFileAttachmentFromUrlAsync(attachment));
        FileAttachment[] attachments = await Task.WhenAll(downloadTasks);
        return new DisposableCollection<FileAttachment>(attachments);
    }

    /// <summary>
    ///     Retrieve a file attachment from a message asynchronously.
    /// </summary>
    /// <param name="attachment">The attachment from which to retrieve the file.</param>
    /// <returns>The FileAttachment object representing the retrieved file attachment.</returns>
    private async Task<FileAttachment> CreateFileAttachmentFromUrlAsync(
        IAttachment attachment)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(attachment.Url);
        response.EnsureSuccessStatusCode();

        // Copy the content of the response stream to a new MemoryStream
        Stream responseStream = await response.Content.ReadAsStreamAsync();
        MemoryStream ms = new MemoryStream();
        await responseStream.CopyToAsync(ms);
        ms.Position = 0;  // Reset the MemoryStream position

        return new FileAttachment(
            ms,
            attachment.Filename,
            attachment.Description,
            attachment.IsSpoiler());
    }
}