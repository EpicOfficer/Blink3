using System.Diagnostics.CodeAnalysis;
using Blink3.Core.Configuration;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;

namespace Blink3.API.Services;

/// <summary>
///     The DiscordStartupService class is responsible for logging in and logging out the Discord bot using the
///     DiscordRestClient.
/// </summary>
[SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
public class DiscordStartupService(
    DiscordRestClient client,
    IOptions<BlinkConfiguration> config,
    ILogger<DiscordStartupService> logger) : IHostedService
{
    /// <summary>
    ///     Represents the configuration settings for the Blink application.
    /// </summary>
    private BlinkConfiguration Config => config.Value;

    /// <summary>
    ///     Starts the asynchronous process of logging in the Discord client with the provided bot token.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.LoginAsync(TokenType.Bot, Config.Discord.BotToken);
        logger.LogInformation("Logged in as {botUser}#{botDiscriminator}.", client.CurrentUser.Username,
            client.CurrentUser.Discriminator);
    }

    /// <summary>
    ///     Stops the Discord startup service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for stopping the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.LogoutAsync();
    }
}