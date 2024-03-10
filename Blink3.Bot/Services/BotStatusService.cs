using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Services;

public class BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger)
    : DiscordClientService(client, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        Logger.LogInformation("Client is ready!");

        await Client.SetActivityAsync(new Game("Blink 3.0"));
    }
}