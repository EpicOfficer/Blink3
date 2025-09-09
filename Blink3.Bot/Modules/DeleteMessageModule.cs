using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Modules;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageMessages)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
[UsedImplicitly]
public class DeleteMessageModule(
    IUnitOfWork unitOfWork,
    IDiscordAttachmentService discordAttachmentService,
    ILogger<DeleteMessageModule> logger)
    : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    [MessageCommand("Delete & log")]
    public async Task DeleteAndLogMessage(IMessage message)
    {
        UserLogContext userLogContext = new(Context.User);
        GuildLogContext guildLogContext = new(Context.Guild);

        using (logger.BeginScope(new { Guild = guildLogContext, User = userLogContext }))
        {
            BlinkGuild guildConfig = await FetchConfig();
            logger.LogInformation("{User} initiated Delete & Log for a message in {Guild}", userLogContext, guildLogContext);

            IMessageChannel? logChannel = await GetValidatedLogChannel(guildConfig, message, userLogContext, guildLogContext);;
            if (logChannel is null) return;

            EmbedBuilder embed = BuildEmbed(message);

            if (message.Attachments.Count > 0)
            {
                logger.LogDebug("Message contains attachments. Downloading attachments...");
                await DeferAsync(true);
                
                using IDisposableCollection<FileAttachment> attachments =
                    await discordAttachmentService.DownloadAsync(message, true);
                
                logger.LogDebug("Attachments downloaded successfully. Sending them to the log channel.");
                await logChannel.SendFilesAsync(attachments, "", embed: embed.Build());
            }
            else
            {
                logger.LogDebug("Message has no attachments. Sending embed to the log channel directly.");
                await logChannel.SendMessageAsync(embed: embed.Build());
            }

            logger.LogInformation("Message successfully logged to {LogChannel} by {User} in {Guild}", 
                logChannel.Id, userLogContext, guildLogContext);
            
            await message.DeleteAsync();
            await RespondSuccessAsync("Message Deleted", "The message has been deleted and logged.");
        }
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

    private async Task<IMessageChannel?> GetValidatedLogChannel(BlinkGuild guildConfig, IMessage fullMessage, UserLogContext userLogContext, GuildLogContext guildLogContext)
    {
        if (guildConfig.LoggingChannelId is null)
        {
            logger.LogWarning("{User} tried to delete & log a message, but no logging channel is set in {Guild}",
                userLogContext, guildLogContext);
            await RespondErrorAsync("No logging channel set",
                "You must set a logging channel before you can delete and log messages.");
            return null;
        }

        IMessageChannel logChannel = await Context.Guild.GetTextChannelAsync(guildConfig.LoggingChannelId.Value);
        if (logChannel is null)
        {
            logger.LogWarning("{User} tried to delete & log a message, but the logging could not be fetched {Guild}", 
                userLogContext, guildLogContext);
            await RespondErrorAsync("Logging channel not found",
                "The logging channel could not be found. Please set a valid logging channel.");
            return null;
        }

        if (logChannel.Id == Context.Channel.Id)
        {
            logger.LogInformation("{User} attempted to delete & log a message, but the logging channel is the same as the current channel {ChannelId} in {Guild}",
                Context.Channel.Id, userLogContext, guildLogContext);
            await RespondErrorAsync("Cannot log to the same channel",
                "You cannot log messages to the same channel you are deleting them from.");
            return null;
        }

        // ReSharper disable once InvertIf
        if (fullMessage.Author.IsBot || fullMessage.Author.IsWebhook)
        {
            logger.LogInformation("{User} tried to delete a message from a bot or webhook in {Guild}", 
                userLogContext, guildLogContext);
            await RespondErrorAsync("Cannot delete bot messages",
                "You cannot delete messages from bots or webhooks.");
            return null;
        }

        return logChannel;
    }
}