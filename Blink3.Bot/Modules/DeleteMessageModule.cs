using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Blink3.Core.Repositories.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageMessages)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
public class DeleteMessageModule(
    IBlinkGuildRepository blinkGuildRepository,
    IDiscordAttachmentService discordAttachmentService)
    : BlinkModuleBase<IInteractionContext>(blinkGuildRepository)
{
    private readonly IBlinkGuildRepository _blinkGuildRepository = blinkGuildRepository;

    [MessageCommand("Delete & log")]
    public async Task DeleteAndLogMessage(IMessage message)
    {
        BlinkGuild guildConfig = await FetchConfig();
        IMessage fullMessage = await Context.Channel.GetMessageAsync(message.Id);

        IMessageChannel? logChannel = await GetValidatedLogChannel(guildConfig, fullMessage);
        if (logChannel is null) return;

        EmbedBuilder embed = BuildEmbed(fullMessage);

        if (fullMessage.Attachments.Count > 0)
        {
            await DeferAsync(true);
            using IDisposableCollection<FileAttachment> attachments =
                await discordAttachmentService.DownloadAsync(message, true);
            await logChannel.SendFilesAsync(attachments, "", embed: embed.Build());
        }
        else
        {
            await logChannel.SendMessageAsync(embed: embed.Build());
        }

        await message.DeleteAsync();
        await RespondSuccessAsync("Message Deleted", "The message has been deleted and logged.");
    }

    private EmbedBuilder BuildEmbed(IMessage fullMessage)
    {
        return new EmbedBuilder()
            .WithAuthor(fullMessage.Author)
            .WithDescription(fullMessage.Content)
            .WithTimestamp(fullMessage.Timestamp)
            .WithFields(new EmbedFieldBuilder().WithName("Deleted by").WithValue($"{Context.User.Mention}"),
                new EmbedFieldBuilder().WithName("Channel").WithValue($"<#{Context.Channel.Id}>"))
            .WithFooter($"User ID: {fullMessage.Author.Id}");
    }

    private async Task<IMessageChannel?> GetValidatedLogChannel(BlinkGuild guildConfig, IMessage fullMessage)
    {
        if (guildConfig.LoggingChannelId is null)
        {
            await RespondErrorAsync("No logging channel set",
                "You must set a logging channel before you can delete and log messages.");
            return null;
        }

        IMessageChannel logChannel = await Context.Guild.GetTextChannelAsync(guildConfig.LoggingChannelId.Value);
        if (logChannel is null)
        {
            await RespondErrorAsync("Logging channel not found",
                "The logging channel could not be found. Please set a valid logging channel.");
            return null;
        }

        if (logChannel.Id == Context.Channel.Id)
        {
            await RespondErrorAsync("Cannot log to the same channel",
                "You cannot log messages to the same channel you are deleting them from.");
            return null;
        }

        // ReSharper disable once InvertIf
        if (fullMessage.Author.IsBot || fullMessage.Author.IsWebhook)
        {
            await RespondErrorAsync("Cannot delete bot messages",
                "You cannot delete messages from bots or webhooks.");
            return null;
        }

        return logChannel;
    }
}