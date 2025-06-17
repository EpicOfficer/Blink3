using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Modules;

[Group("temp", "Create and manage temporary VCs")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.Connect)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
public class TempVcModule(
    IUnitOfWork unitOfWork,
    ILogger<TempVcModule> logger)
    : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private const string CamOnlyIcon = "\ud83d\udcf7";
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [SlashCommand("create", "Create")]
    public async Task Create(string? name = null)
    {
        BlinkGuild guildConfig = await FetchConfig();
        if (guildConfig.TemporaryVcCategoryId is null)
        {
            await RespondErrorAsync("Not configured", "Temporary VCs have not been configured for this server.");
            return;
        }

        if (await _unitOfWork.TempVcRepository.GetByUserIdAsync(Context.Guild.Id, Context.User.Id) is not null)
        {
            await RespondErrorAsync("Temporary VC limit reached", "You already have a temporary VC in this server.");
            return;
        }

        name ??= $"{Context.User.GetFriendlyName()}'s VC";
        IVoiceChannel? voiceChannel =
            await Context.Guild.CreateVoiceChannelAsync(name,
                vc => { vc.CategoryId = guildConfig.TemporaryVcCategoryId; });

        if (voiceChannel is null)
        {
            await RespondErrorAsync("Error creating VC", "An error has occured while creating a temporary VC.");
            return;
        }

        await _unitOfWork.TempVcRepository.AddAsync(new TempVc
        {
            GuildId = Context.Guild.Id,
            UserId = Context.User.Id,
            ChannelId = voiceChannel.Id
        });
        await _unitOfWork.SaveChangesAsync();

        ComponentBuilderV2 builder = new(new ContainerBuilder()
            .WithAccentColor(Colours.Info)
            .WithTextDisplay($"""
                              ## Channel created
                              Hey {Context.User.Mention}, your temporary voice channel `{voiceChannel.Name}` has been created!
                              """)
            .WithSeparator(isDivider: false)
            .WithActionRow(new ActionRowBuilder()
                .WithButton("Take me there", style: ButtonStyle.Link, url: $"https://discord.com/channels/{Context.Guild.Id}/{voiceChannel.Id}")
            )
        );
        
        await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: new AllowedMentions(AllowedMentionTypes.Users), ephemeral: false);
    }

    [SlashCommand("rename", "Rename your Temporary VC")]
    public async Task Rename(string? name = null)
    {
        (TempVc? tempVc, IVoiceChannel? voiceChannel) = await GetTempVcAndVoiceChannelAsync();
        if (tempVc is null || voiceChannel is null)
            return;

        try
        {
            name ??= $"{Context.User.GetFriendlyName()}'s VC";
            name = name.Trim();
            if (voiceChannel.Name != name)
                await voiceChannel.ModifyAsync(vc => { vc.Name = name; });
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Failed to rename VC {channel} for user {user} in guild {guild}", voiceChannel.Id,
                Context.User.Id, Context.Guild.Id);
            await RespondErrorAsync("Unable to rename Temporary VC",
                "An error occurred while trying to rename the channel.  Try again in a few minutes!");
            return;
        }

        await RespondSuccessAsync("Temporary VC Renamed",
            $"Your temporary VC has been renamed to {voiceChannel.Mention}");
    }

    [SlashCommand("ban", "Ban a member from your Temporary VC")]
    [UserCommand("Ban from VC")]
    public async Task Ban(SocketGuildUser user)
    {
        (TempVc? tempVc, IVoiceChannel? voiceChannel) = await GetTempVcAndVoiceChannelAsync();
        if (tempVc is null || voiceChannel is null)
            return;

        if (user.Id == Context.Client.CurrentUser.Id)
        {
            await RespondErrorAsync("Cannot ban the bot", "You cannot ban me from your temporary VC!");
            return;
        }
        
        if (user.Id == Context.User.Id)
        {
            await RespondErrorAsync("Cannot ban yourself", "You cannot ban yourself from your temporary VC!");
            return;
        }
        
        if (!tempVc.BannedUsers.Contains(user.Id)) tempVc.BannedUsers.Add(user.Id);
        await _unitOfWork.TempVcRepository.UpdateAsync(tempVc);
        await _unitOfWork.SaveChangesAsync();
        
        if (user.VoiceChannel?.Id == voiceChannel.Id)
            await user.ModifyAsync(u => u.Channel = null);

        await voiceChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(
            connect: PermValue.Deny,
            sendMessages: PermValue.Deny));
        
        await RespondSuccessAsync("User banned", $"{user.Mention} has been banned from your temporary VC.", ephemeral: false);
    }

    [SlashCommand("unban", "Unban a member from your Temporary VC")]
    [UserCommand("Unban from VC")]
    public async Task Unban(SocketGuildUser user)
    {
        (TempVc? tempVc, IVoiceChannel? voiceChannel) = await GetTempVcAndVoiceChannelAsync();
        if (tempVc is null || voiceChannel is null)
            return;

        if (!tempVc.BannedUsers.Contains(user.Id))
        {
            await RespondErrorAsync("User not banned", $"{user.Mention} is not banned from your temporary VC.");
            return;
        }
        
        tempVc.BannedUsers.Remove(user.Id);
        await _unitOfWork.TempVcRepository.UpdateAsync(tempVc);
        await _unitOfWork.SaveChangesAsync();
        await voiceChannel.RemovePermissionOverwriteAsync(user);
        
        await RespondSuccessAsync("User unbanned", $"{user.Mention} has been unbanned from your temporary VC.", ephemeral: false);
    }
    
    [SlashCommand("limit", "Set a user limit for your Temporary VC")]
    public async Task SetLimit([MaxValue(25)] [MinValue(0)] int limit = 0)
    {
        (TempVc? tempVc, IVoiceChannel? voiceChannel) = await GetTempVcAndVoiceChannelAsync();
        if (tempVc is null || voiceChannel is null)
            return;

        if (limit is 1 or < 0) limit = 2;
        if (limit > 24) limit = 25;
        await voiceChannel.ModifyAsync(v => v.UserLimit = limit);
        await RespondSuccessAsync("Channel limit set", $"Your temporary VC channel limit has been set to {limit}");
    }

    [SlashCommand("camonly", "Toggle camera's only mode for your Temporary VC")]
    public async Task CamOnly()
    {
        (TempVc? tempVc, IVoiceChannel? voiceChannel) = await GetTempVcAndVoiceChannelAsync();
        if (tempVc is null || voiceChannel is null)
            return;

        await _unitOfWork.TempVcRepository.UpdatePropertiesAsync(tempVc, vc => vc.CamOnly = !vc.CamOnly);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            switch (tempVc.CamOnly)
            {
                case true when !voiceChannel.Name.StartsWith(CamOnlyIcon):
                    await voiceChannel.ModifyAsync(vc => vc.Name = $"{CamOnlyIcon} {voiceChannel.Name}");
                    break;
                case false when voiceChannel.Name.StartsWith(CamOnlyIcon):
                    await voiceChannel.ModifyAsync(vc =>
                        vc.Name = voiceChannel.Name.TrimStart(CamOnlyIcon.ToCharArray()));
                    break;
            }
        }
        catch
        {
            // ignored
        }

        await RespondSuccessAsync("Temporary VC updated",
            $"Camera only mode has been {(tempVc.CamOnly ? "enabled" : "disabled")}.");
    }

    [SlashCommand("delete", "Delete your Temporary VC")]
    public async Task Delete()
    {
        TempVc? tempVc = await GetTempVcAsync();
        if (tempVc is null) return;

        if (await Context.Guild.GetVoiceChannelAsync(tempVc.ChannelId) is { } voiceChannel)
            await voiceChannel.DeleteAsync();

        await _unitOfWork.TempVcRepository.DeleteAsync(tempVc);
        await _unitOfWork.SaveChangesAsync();
        await RespondSuccessAsync("Channel deleted", "Your temporary VC has been successfully deleted.");
    }

    private async Task<TempVc?> GetTempVcAsync()
    {
        TempVc? tempVc = await _unitOfWork.TempVcRepository.GetByUserIdAsync(Context.Guild.Id, Context.User.Id);
        if (tempVc is not null) return tempVc;

        await RespondErrorAsync("No Temporary VC", "You do not have a temporary VC in this server.");
        return null;
    }

    private async Task<(TempVc?, IVoiceChannel?)> GetTempVcAndVoiceChannelAsync()
    {
        TempVc? tempVc = await GetTempVcAsync();
        if (tempVc is null) return (null, null);

        IVoiceChannel? voiceChannel = await Context.Guild.GetVoiceChannelAsync(tempVc.ChannelId);
        if (voiceChannel is not null) return (tempVc, voiceChannel);

        await RespondErrorAsync("Temporary VC not found", "Your temporary VC could not be found.");
        return (null, null);
    }
}