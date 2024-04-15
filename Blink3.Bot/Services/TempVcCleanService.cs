using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Repositories.Interfaces;
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
    private readonly ILogger<DiscordClientService> _logger = logger;
    private Timer? _timer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        _logger.LogInformation("Temp VC cleaning service is starting...");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
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
        ITempVcRepository tempVcRepository = scope.ServiceProvider.GetRequiredService<ITempVcRepository>();
        IReadOnlyCollection<TempVc> tempVcs = await tempVcRepository.GetAllAsync();

        Dictionary<ulong, List<TempVc>> guilds = tempVcs.GroupBy(tempVc => tempVc.GuildId)
            .ToDictionary(group => group.Key, group => group.ToList());

        _logger.LogInformation("Cleaning {count} unique VCs in {guildCount} guilds",
            tempVcs.Count, guilds.Count);

        foreach (KeyValuePair<ulong, List<TempVc>> guild in guilds)
            await HandleGuild(guild.Key, guild.Value, tempVcRepository);
    }

    private async Task HandleGuild(ulong guildId, List<TempVc> tempVcs, ITempVcRepository tempVcRepository)
    {
        if (Client.GetGuild(guildId) is not { } guild) return;

        foreach (TempVc tempVc in tempVcs) await HandleChannel(guild, tempVc, tempVcRepository);
    }

    private async Task HandleChannel(SocketGuild guild, TempVc tempVc, ITempVcRepository tempVcRepository)
    {
        // If the VC was created less than 2 minutes ago, skip it
        if (tempVc.CreatedAt.AddMinutes(2) > DateTime.UtcNow) return;

        // If the VC is missing, delete it from the database
        if (guild.GetVoiceChannel(tempVc.ChannelId) is not { } channel)
        {
            _logger.LogInformation(
                "Deleting temp VC {channelId} in guild {guildId} from database as the channel is missing",
                tempVc.ChannelId, guild.Id);
            await tempVcRepository.DeleteAsync(tempVc);
            return;
        }

        await HandleChannelUsers(guild, channel, tempVc, tempVcRepository);
    }

    private async Task HandleChannelUsers(SocketGuild guild, SocketVoiceChannel channel, TempVc tempVc,
        ITempVcRepository tempVcRepository)
    {
        IReadOnlyCollection<SocketGuildUser>? connectedUsers = channel.ConnectedUsers;

        if (tempVc.CamOnly)
        {
            // Kick users who are not videoing, are not bots, and do not have manage messages permission
            IEnumerable<SocketGuildUser> membersToKick = connectedUsers.Where(u =>
                u is { IsBot: false, IsVideoing: false } && !u.GetPermissions(channel).ManageMessages);

            foreach (SocketGuildUser user in membersToKick)
            {
                _logger.LogInformation(
                    "Kicking user {userId} from VC {channelId} in guild {guildId} as they are not videoing",
                    user.Id, channel.Id, new { guild.Id, guild.Name });
                await user.ModifyAsync(u => u.Channel = null);
                try
                {
                    await user.SendMessageAsync(
                        $"You were kicked from the temporary voice channel {channel.Name} in {guild.Name} as you did not have your camera enabled.");
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Failed to DM user {userId} in guild {guildId}", user.Id, guild.Id);
                }
            }
        }

        // If there are non-bot users in the VC, skip it
        if (connectedUsers.Any(u => !u.IsBot)) return;

        // Delete the VC, as there are no users in it
        _logger.LogInformation("Automatically deleting stale VC {@Channel} in {@Guild}",
            tempVc, new { guild.Id, guild.Name });
        await channel.DeleteAsync();
        await tempVcRepository.DeleteAsync(tempVc);
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