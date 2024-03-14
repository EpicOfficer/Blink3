using System.Reflection;
using Blink3.Bot.MessageStyles.Extensions;
using Blink3.Bot.MessageStyles.Variations;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Services;

public class InteractionHandler(
    DiscordSocketClient client,
    ILogger<DiscordClientService> logger,
    InteractionService handler,
    IServiceProvider provider)
    : DiscordClientService(client, logger)
{
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

        // Register the commands globally.
        await handler.RegisterCommandsToGuildAsync(787646005641216040);
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
            logger.LogError(e, "Exception occurred whilst attempting to handle interaction.");
            
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
    
    private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result?.IsSuccess is not true)
        {
            logger.LogWarning("Error handling interaction {interaction} in module {module} for user {userId}: {ErrorReason}", commandInfo?.Name, commandInfo?.Module?.Name, context?.User?.Id, result?.ErrorReason);
        }
        else
        {
            logger.LogInformation("Handled interaction {interaction} in module {module} for user {userId}", commandInfo.Name, commandInfo.Module.Name, context.User.Id);
        }
        
        Embed embed = new EmbedBuilder()
            .WithStyle(result is { IsSuccess: true } ? new SuccessStyle() : new ErrorStyle())
            .WithDescription(result?.ErrorReason)
            .Build();

        if (context.Interaction.HasResponded)
            await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
        else
            await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
    }
}