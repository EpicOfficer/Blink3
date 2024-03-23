using System.Diagnostics.CodeAnalysis;
using Blink3.Common.Configuration;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;

namespace Blink3.API.Services;

[SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
public class DiscordStartupService(
    DiscordRestClient client,
    IOptions<BlinkConfiguration> config,
    ILogger<DiscordStartupService> logger) : IHostedService
{
    private BlinkConfiguration Config => config.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.LoginAsync(TokenType.Bot, Config.Discord.BotToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.LogoutAsync();
    }
}