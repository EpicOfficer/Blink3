using System.Diagnostics;
using Blink3.Bot.Extensions;
using Blink3.Common.Extensions;
using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageMessages)]
[RequireBotPermission(GuildPermission.ManageMessages | GuildPermission.ManageWebhooks)]
public class MoveMessageModule(IHttpClientFactory httpClientFactory) : BlinkModuleBase<IInteractionContext>
{
    [MessageCommand("Move message")]
    public async Task MoveMessageFrom(SocketMessage message)
    {
        SelectMenuBuilder? builder = new SelectMenuBuilder()
            .WithCustomId($"blink-move-message_{Context.Channel.Id}_{message.Id}")
            .WithType(ComponentType.ChannelSelect)
            .WithChannelTypes(new List<ChannelType> { ChannelType.Text });

        ComponentBuilder? componentBuilder = new ComponentBuilder().WithSelectMenu(builder);

        await RespondPlainAsync("Select a channel to move this message to",
            components: componentBuilder.Build());
    }

    [ComponentInteraction("blink-move-message_*_*")]
    public async Task MoveMessageTo(string channelIdStr, string messageIdStr, SocketChannel[] channels)
    {
        await DeferAsync(true);

        if (!TryGetSourceAndTargetChannels(channelIdStr, channels,
                out SocketTextChannel? sourceChannel,
                out SocketTextChannel? targetChannel,
                out string? errorMessage))
        {
            await RespondErrorAsync(message: errorMessage);
            return;
        }

        Debug.Assert(sourceChannel is not null, nameof(sourceChannel) + " != null");
        Debug.Assert(targetChannel is not null, nameof(targetChannel) + " != null");
        IMessage? message = await GetMessageToMove(sourceChannel, messageIdStr.ToUlong());
        if (message is null) return;

        await MoveMessage(message, sourceChannel, targetChannel);
    }

    /// <summary>
    ///     Tries to get the source and target channels asynchronously.
    /// </summary>
    /// <param name="channelIdStr">The ID of the source channel as a string.</param>
    /// <param name="channels">The array of target channels.</param>
    /// <param name="sourceChannel">When this method returns, contains the source channel if found; otherwise, null.</param>
    /// <param name="targetChannel">When this method returns, contains the target channel if found; otherwise, null.</param>
    /// <returns>true if the source and target channels were successfully obtained; otherwise, false.</returns>
    private bool TryGetSourceAndTargetChannels(string channelIdStr,
        SocketChannel[] channels,
        out SocketTextChannel? sourceChannel,
        out SocketTextChannel? targetChannel,
        out string? errorMessage)
    {
        ulong sourceChannelId = channelIdStr.ToUlong();

        sourceChannel = null;
        targetChannel = null;
        errorMessage = null;

        if (Context.Guild.GetChannelAsync(sourceChannelId).Result is not SocketTextChannel source ||
            channels.FirstOrDefault() is not SocketTextChannel target)
        {
            errorMessage = "Could not get either the source or target channel to move message.";
            return false;
        }

        if (source.Id == target.Id)
        {
            errorMessage = "Source and target channels cannot be the same.";
            return false;
        }

        sourceChannel = source;
        targetChannel = target;

        return true;
    }

    /// <summary>
    ///     Gets the message to move from the specified source channel and message ID.
    /// </summary>
    /// <param name="sourceChannel">The source text channel.</param>
    /// <param name="messageId">The ID of the message to move.</param>
    /// <returns>
    ///     The message to move, or null if the message is not found or is authored by a bot.
    /// </returns>
    private async Task<IMessage?> GetMessageToMove(SocketTextChannel sourceChannel, ulong messageId)
    {
        IMessage message = await sourceChannel.GetMessageAsync(messageId);

        if (!message.Author.IsBot) return message;

        await RespondErrorAsync("Unsupported message", "This command cannot be used on bot messages");
        return null;
    }

    /// <summary>
    ///     Moves a message from the source channel to the target channel.
    /// </summary>
    /// <param name="message">The message to be moved.</param>
    /// <param name="sourceChannel">The source channel from which the message should be moved.</param>
    /// <param name="targetChannel">The target channel to which the message should be moved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task MoveMessage(IMessage message, SocketTextChannel sourceChannel, SocketTextChannel targetChannel)
    {
        IWebhook webhook = await GetOrCreateWebhook(targetChannel);
        using DiscordWebhookClient client = new(webhook);

        if (message.Attachments.Count is not 0)
            await SendMessageWithAttachments(client, message);
        else
            await SendMessageWithoutAttachments(client, message);

        await RespondSuccessAsync("Message moved", $"Successfully moved message to {targetChannel.Name}");
    }

    /// <summary>
    ///     Gets or creates a webhook for the specified channel.
    /// </summary>
    /// <param name="channel">The channel to get or create the webhook for.</param>
    /// <returns>The webhook object.</returns>
    private static async Task<IWebhook> GetOrCreateWebhook(SocketTextChannel channel)
    {
        return (await channel.GetWebhooksAsync()).FirstOrDefault() ?? await channel.CreateWebhookAsync("Blink");
    }

    /// <summary>
    ///     Sends a message with attachments using a DiscordWebhookClient.
    /// </summary>
    /// <param name="client">The DiscordWebhookClient to send the message with attachments.</param>
    /// <param name="message">The message with attachments to send.</param>
    private async Task SendMessageWithAttachments(DiscordWebhookClient client, IMessage message)
    {
        List<FileAttachment> attachments = await GetAttachmentsFromMessage(message);

        await client.SendFilesAsync(attachments,
            message.Content,
            username: message.Author.GetFriendlyName(),
            avatarUrl: message.Author.GetDisplayAvatarUrl());
    }

    /// <summary>
    ///     Sends a message without attachments using a DiscordWebhookClient.
    /// </summary>
    /// <param name="client">The DiscordWebhookClient used to send the message.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task SendMessageWithoutAttachments(DiscordWebhookClient client, IMessage message)
    {
        await client.SendMessageAsync(message.Content,
            username: message.Author.GetFriendlyName(),
            avatarUrl: message.Author.GetDisplayAvatarUrl());
    }

    /// <summary>
    ///     Gets the attachments from a given Discord message.
    /// </summary>
    /// <param name="message">The Discord message.</param>
    /// <returns>A list of file attachments.</returns>
    private async Task<List<FileAttachment>> GetAttachmentsFromMessage(IMessage message)
    {
        List<FileAttachment> attachments = [];
        using HttpClient httpClient = httpClientFactory.CreateClient();

        foreach (IAttachment? attachment in message.Attachments)
        {
            HttpResponseMessage response = await httpClient.GetAsync(attachment.Url);
            response.EnsureSuccessStatusCode();
            Stream stream = await response.Content.ReadAsStreamAsync();

            attachments.Add(new FileAttachment(stream,
                attachment.Filename,
                attachment.Description,
                attachment.IsSpoiler()));
        }

        return attachments;
    }
}