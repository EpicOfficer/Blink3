using System.Reflection;
using Blink3.Bot.MessageStyles.Extensions;
using Blink3.Bot.MessageStyles.Variations;
using Blink3.Common.Configuration;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blink3.Bot.Services;

public class InteractionHandler(
    DiscordSocketClient client,
    ILogger<DiscordClientService> logger,
    InteractionService handler,
    IServiceProvider provider,
    IOptions<BlinkConfiguration> config)
    : DiscordClientService(client, logger)
{
    private readonly ILogger<DiscordClientService> _logger = logger;
    private BlinkConfiguration Config => config.Value;

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

    private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result?.IsSuccess is true)
        {
            _logger.LogInformation("Handled interaction {interaction} in module {module} for user {userId}",
                commandInfo.Name, commandInfo.Module.Name, context.User.Id);
            return;
        }

        _logger.LogWarning(
            "Error handling interaction {interaction} in module {module} for user {userId}: {ErrorReason}",
            commandInfo?.Name, commandInfo?.Module?.Name, context?.User?.Id, result?.ErrorReason);

        if (context?.Interaction is null) return;

        Embed embed = new EmbedBuilder()
            .WithStyle(new ErrorStyle())
            .WithDescription(result?.ErrorReason)
            .Build();

        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
        else
            await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
    }
}