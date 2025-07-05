using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Services;

public class TempVcCleanService(
    DiscordSocketClient client,
    ILogger<DiscordClientService> logger,
    IServiceScopeFactory scopeFactory)
    : DiscordClientService(client, logger)
{
    private const int TimerInterval = 2;
    private readonly ILogger<DiscordClientService> _logger = logger;
    private Timer? _timer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        _logger.LogInformation("Temp VC cleaning service is starting...");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(TimerInterval));
    }

    private void DoWork(object? state)
    {
        try
        {
            DoWorkAsync().Forget();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while cleaning temp VCs");
        }
    }

    private async Task DoWorkAsync()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IReadOnlyCollection<TempVc> tempVcs = await unitOfWork.TempVcRepository.GetAllAsync();

        Dictionary<ulong, List<TempVc>> guilds = tempVcs.GroupBy(tempVc => tempVc.GuildId)
            .ToDictionary(group => group.Key, group => group.ToList());

        _logger.LogDebug("Cleaning {count} unique VCs in {guildCount} guilds",
            tempVcs.Count, guilds.Count);
        
        foreach (KeyValuePair<ulong, List<TempVc>> guild in guilds)
            await HandleGuild(guild.Key, guild.Value, unitOfWork);
    }

    private async Task HandleGuild(ulong guildId, List<TempVc> tempVcs, IUnitOfWork unitOfWork)
    {
        if (Client.GetGuild(guildId) is not { } guild) return;

        GuildLogContext guildLogContext = new(guild);

        using (_logger.BeginScope(new { Guild = guildLogContext }))
        {
            foreach (TempVc tempVc in tempVcs) await HandleChannel(guild, tempVc, unitOfWork);
        }
    }

    private async Task HandleChannel(SocketGuild guild, TempVc tempVc, IUnitOfWork unitOfWork)
    {
        // If the VC is missing, delete it from the database
        if (guild.GetVoiceChannel(tempVc.ChannelId) is not { } channel)
        {
            GuildLogContext guildContext = new(guild);
            _logger.LogInformation(
                "Deleting temp VC {channelId} in {Guild} from database as the channel is missing",
                tempVc.ChannelId, guildContext);
            await unitOfWork.TempVcRepository.DeleteAsync(tempVc);
            await unitOfWork.SaveChangesAsync();
            return;
        }

        await HandleChannelUsers(guild, channel, tempVc, unitOfWork);
    }

    private async Task HandleChannelUsers(SocketGuild guild, SocketVoiceChannel channel, TempVc tempVc,
        IUnitOfWork unitOfWork)
    {
        IReadOnlyCollection<SocketGuildUser>? connectedUsers = channel.ConnectedUsers;

        if (tempVc.BannedUsers.Count > 0)
        {
            IEnumerable<SocketGuildUser> membersToKick = connectedUsers.Where(u =>
                tempVc.BannedUsers.Contains(u.Id));
            
            foreach (SocketGuildUser user in membersToKick)
            {
                UserLogContext userContext = new(user);
                GuildChannelLogContext channelContext = new(channel);

                _logger.LogInformation("Kicking {User} from {Channel} as they are banned from this VC",
                    userContext, channelContext);

                await user.ModifyAsync(u => u.Channel = null);
            }
        }
        
        if (tempVc.CamOnly)
        {
            // Kick users who are not videoing, are not bots, and do not have manage messages permission
            IEnumerable<SocketGuildUser> membersToKick = connectedUsers.Where(u =>
                u is { IsBot: false, IsVideoing: false } && !u.GetPermissions(channel).ManageMessages);

            foreach (SocketGuildUser user in membersToKick)
            {
                UserLogContext userContext = new(user);
                GuildChannelLogContext channelContext = new(channel);
                _logger.LogInformation("Kicking {User} from {Channel;} as they are not videoing", userContext, channelContext);
                await user.ModifyAsync(u => u.Channel = null);
                try
                {
                    await user.SendMessageAsync(
                        $"You were kicked from the temporary voice channel {channel.Name} in {guild.Name} as you did not have your camera enabled.");
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Failed to DM {User} in {Channel}", userContext, channelContext);
                }
            }
        }
        
        // If the VC is less than 2 minutes old, skip it
        if (tempVc.CreatedAt.AddMinutes(2) > DateTime.UtcNow) return;

        // If there are non-bot users in the VC, skip it
        if (connectedUsers.Any(u => !u.IsBot)) return;

        // Delete the VC, as there are no users in it
        _logger.LogInformation("Automatically deleting stale VC {Channel}", new GuildChannelLogContext(channel));
        await channel.DeleteAsync();
        await unitOfWork.TempVcRepository.DeleteAsync(tempVc);
        await unitOfWork.SaveChangesAsync();
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}