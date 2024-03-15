using System.Diagnostics.CodeAnalysis;
using Blink3.Common.Configuration;
using Discord;
using Discord.Rest;

namespace Blink3.API.Services;

[SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
public class DiscordStartupService(DiscordRestClient client, BlinkConfiguration config, ILogger<DiscordStartupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.LoginAsync(TokenType.Bot, config.Discord.BotToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.LogoutAsync();
    }
}