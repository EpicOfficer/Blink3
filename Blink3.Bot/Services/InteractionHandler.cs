using System.Reflection;
using Blink3.Bot.MessageStyles.Extensions;
using Blink3.Bot.MessageStyles.Variations;
using Blink3.Core.Configuration;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blink3.Bot.Services;

/// <summary>
///     Handles interactions and executes interaction commands.
/// </summary>
public class InteractionHandler(
    DiscordSocketClient client,
    ILogger<DiscordClientService> logger,
    InteractionService handler,
    IServiceProvider provider,
    IOptions<BlinkConfiguration> config)
    : DiscordClientService(client, logger)
{
    /// <summary>
    ///     This variable represents the logger used for logging messages and errors in the InteractionHandler class.
    /// </summary>
    private readonly ILogger<DiscordClientService> _logger = logger;

    /// <summary>
    ///     Represents the configuration settings for the application.
    /// </summary>
    private BlinkConfiguration Config => config.Value;

    /// <summary>
    ///     Executes the asynchronous operation for interaction handling.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        IServiceScope scope = provider.CreateScope();
        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);

        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += HandleInteraction;

        // Also process the result of the command execution.
        handler.InteractionExecuted += HandleInteractionExecute;

        await Client.WaitForReadyAsync(cancellationToken);

        // Register the commands.
        if (Config.Discord.DevGuildId.HasValue)
        {
            _logger.LogInformation("Registering commands to guild {guildId}", Config.Discord.DevGuildId);
            await handler.RegisterCommandsToGuildAsync(Config.Discord.DevGuildId.Value);
        }
        else
        {
            _logger.LogInformation("Registering commands globally");
            await handler.RegisterCommandsGloballyAsync();
        }
    }

    /// <summary>
    ///     Handles the interactions received from Discord.
    /// </summary>
    /// <param name="interaction">The SocketInteraction object representing the received interaction.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            SocketInteractionContext context = new(Client, interaction);

            // Execute the incoming command.
            await handler.ExecuteCommandAsync(context, provider);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occurred whilst attempting to handle interaction.");

            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    /// <summary>
    ///     Executes the handler for an interaction.
    /// </summary>
    /// <param name="commandInfo">The information about the executed command.</param>
    /// <param name="context">The interaction context containing the details of the interaction.</param>
    /// <param name="result">The result of executing the interaction command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            _logger.LogInformation("Handled interaction {interaction} in module {module} for user {userId}",
                commandInfo.Name, commandInfo.Module.Name, context.User.Id);
            return;
        }

        _logger.LogWarning(
            "Error handling interaction {interaction} in module {module} for user {userId}: {ErrorReason}",
            commandInfo.Name, commandInfo.Module.Name, context.User.Id, result.ErrorReason);

        if (context.Interaction is null) return;

        Embed embed = new EmbedBuilder()
            .WithStyle(new ErrorStyle())
            .WithDescription(result.ErrorReason)
            .Build();

        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
        else
            await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
    }
}