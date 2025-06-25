using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Services;

/// <summary>
///     Service for managing the status of the bot.
/// </summary>
public class BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger)
    : DiscordClientService(client, logger)
{
    /// <summary>
    ///     Executes async method to set the bot status and wait for the client to be ready.
    /// </summary>
    /// <param name="stoppingToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        await Client.SetActivityAsync(new Game("blinkbot.io"));
    }
}