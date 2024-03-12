using Discord;
using Discord.Rest;

namespace Blink3.API.Services;

public class DiscordStartupService(DiscordRestClient client, IConfiguration config, ILogger<DiscordStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.LoginAsync(TokenType.Bot, config["Token"]!);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.LogoutAsync();
    }
}