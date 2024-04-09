using Blink3.Core.Interfaces;
using Discord;

namespace Blink3.Bot.Services;

/// <inheritdoc />
public class DiscordAttachmentService(HttpClient httpClient) : IDiscordAttachmentService
{
    public async Task<IEnumerable<FileAttachment>> DownloadAttachmentsFromMessageAsync(IMessage message)
    {
        IEnumerable<Task<FileAttachment>> downloadTasks = message.Attachments.Select(
            async attachment => await CreateFileAttachmentFromUrlAsync(attachment));

        return (await Task.WhenAll(downloadTasks)).ToList();
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

        Stream stream = await response.Content.ReadAsStreamAsync();

        return new FileAttachment(
            stream,
            attachment.Filename,
            attachment.Description,
            attachment.IsSpoiler());
    }
}