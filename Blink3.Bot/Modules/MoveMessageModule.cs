using Blink3.Bot.Extensions;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageMessages)]
[RequireBotPermission(GuildPermission.ManageMessages | GuildPermission.ManageWebhooks)]
public class MoveMessageModule(IDiscordAttachmentService discordAttachmentService)
    : BlinkModuleBase<IInteractionContext>
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

        Result<Tuple<SocketTextChannel, SocketTextChannel>> channelsResult =
            await GetSourceAndTargetChannelsAsync(channelIdStr, channels);

        if (!channelsResult.IsSuccess)
        {
            await RespondErrorAsync(message: channelsResult.Error ?? "An unknown error occured moving the message");
            return;
        }

        SocketTextChannel sourceChannel = channelsResult.SafeValue().Item1;
        SocketTextChannel targetChannel = channelsResult.SafeValue().Item2;

        IMessage? message = await GetMessageToMove(sourceChannel, messageIdStr.ToUlong());
        if (message is null) return;

        await MoveMessage(message, targetChannel);
    }

    /// <summary>
    ///     Retrieves the source and target channels for moving a message.
    /// </summary>
    /// <param name="channelIdStr">The ID of the source channel as a string.</param>
    /// <param name="channels">The collection of socket channels to search for the target channel.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> object with the source and target channels as a tuple if successful,
    ///     otherwise a failed result with an error message indicating the reason for the failure.
    /// </returns>
    private async Task<Result<Tuple<SocketTextChannel, SocketTextChannel>>> GetSourceAndTargetChannelsAsync(
        string channelIdStr,
        IEnumerable<SocketChannel> channels)
    {
        ulong sourceChannelId = channelIdStr.ToUlong();

        if (await Context.Guild.GetChannelAsync(sourceChannelId) is not SocketTextChannel source ||
            channels.FirstOrDefault() is not SocketTextChannel target)
            return Result<Tuple<SocketTextChannel, SocketTextChannel>>.Fail(
                "Could not get either the source or target channel to move message.");

        if (source.Id == target.Id)
            return Result<Tuple<SocketTextChannel, SocketTextChannel>>.Fail(
                "Source and target channels cannot be the same.");

        return Result<Tuple<SocketTextChannel, SocketTextChannel>>.Ok(
            new Tuple<SocketTextChannel, SocketTextChannel>(source, target));
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
    /// <param name="targetChannel">The target channel to which the message should be moved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task MoveMessage(IMessage message, SocketTextChannel targetChannel)
    {
        IWebhook webhook = await GetOrCreateWebhook(targetChannel);
        using DiscordWebhookClient client = new(webhook);

        if (message.Attachments.Count is not 0)
        {
            using IDisposableCollection<FileAttachment> attachments =
                await discordAttachmentService.DownloadAsync(message);
            await SendMessageWithAttachments(client, message, attachments);
        }
        else
        {
            await SendMessageWithoutAttachments(client, message);
        }

        await message.DeleteAsync();

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
    ///     Sends a message with attachments using a Discord webhook client.
    /// </summary>
    /// <param name="client">The Discord webhook client.</param>
    /// <param name="message">The Discord message.</param>
    /// <param name="attachments">The attachments to send with the message.</param>
    private static async Task SendMessageWithAttachments(DiscordWebhookClient client,
        IMessage message,
        IEnumerable<FileAttachment> attachments)
    {
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
}