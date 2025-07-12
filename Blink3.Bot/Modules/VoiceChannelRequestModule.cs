using System.ComponentModel;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Interfaces;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ContextType = Discord.Commands.ContextType;

namespace Blink3.Bot.Modules;

[Discord.Commands.RequireContext(ContextType.Guild)]
[Discord.Commands.RequireUserPermission(GuildPermission.Connect)]
[CommandContextType(InteractionContextType.Guild)]
[IntegrationType(ApplicationIntegrationType.GuildInstall)]
public class VoiceChannelRequestModule(IUnitOfWork unitOfWork) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    [SlashCommand("vc-request", "Request to join somebody's voice channel")]
    public async Task RequestVoiceChannelAsync(SocketGuildUser user)
    {
        if (Context.User is not SocketGuildUser requester)
        {
            await RespondErrorAsync("You must be in a guild to use this command");
            return;
        }

        /*
        if (requester.VoiceChannel is null)
        {
            await RespondErrorAsync("You must be in a voice channel to use this command");
            return;
        }
        
        if (user.VoiceChannel is null)
        {
            await RespondErrorAsync("The user you are trying to request to join is not in a voice channel");
            return;
        }
        
        if (user.VoiceChannel.Id == requester.VoiceChannel.Id)
        {
            await RespondErrorAsync("You are already in this channel.");
            return;
        }

        if (requester.GetPermissions(user.VoiceChannel).ViewChannel is false)
        {
            await RespondErrorAsync("You do not have permission to join this channel");
            return;
        }*/
        
        ContainerBuilder? container = new ContainerBuilder()
            .WithAccentColor(Colours.Info)
            .WithTextDisplay($"""
                              ## Request to join {user.VoiceChannel?.Name}
                              Hey {requester.Mention}, {user.Mention} has requested to join your voice channel.
                              """)
            .WithSeparator(isDivider: false)
            .WithActionRow(new ActionRowBuilder()
                .WithButton("Accept", $"vc-request-accept_{user.Id}", ButtonStyle.Success)
                .WithButton("Decline", $"vc-request-decline_{user.Id}", ButtonStyle.Danger));
        
        ComponentBuilderV2 builder = new(container);
        await RespondOrFollowUpAsync(components: builder.Build(), ephemeral: true);
    }
    
    [ComponentInteraction("vc-request-accept_*", true)]
    public async Task AcceptRequest(ulong targetId)
    {
        if (Context.User is not SocketGuildUser target)
        {
            await RespondErrorAsync("You must be in a guild to use this command");
            return;
        }
        
        if (target.Id != targetId)
        {
            await RespondErrorAsync("You cannot accept this request", "This request is not for you!");
            return;
        }
        
        SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
        IMessageInteractionMetadata? meta = component.Message.InteractionMetadata;
        if (meta is null || meta.CreatedAt.AddMinutes(2) < DateTime.UtcNow)
        {
            await RespondErrorAsync("Request expired", "The request to join this voice channel has expired.");
            return;
        }

        IGuildUser? user = Context.User as IGuildUser;
        ContainerBuilder? container = new ContainerBuilder()
            .WithAccentColor(Colours.Info)
            .WithTextDisplay($"""
                              ## Request accepted!
                              {target.Mention} has accepted {user?.Mention}'s request to join their voice channel.
                              {user?.VoiceChannel?.Name}
                              """);
        
        ComponentBuilderV2 builder = new(container);
        await RespondOrFollowUpAsync(components: builder.Build(), ephemeral: false);
    }
}