using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
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
        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), provider);

        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += HandleInteraction;

        // Also process the result of the command execution.
        handler.InteractionExecuted += HandleInteractionExecute;

        await Client.WaitForReadyAsync(cancellationToken);

        // Register the commands globally.
        await handler.RegisterCommandsGloballyAsync();
    }
    
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(Client, interaction);

            // Execute the incoming command.
            var result = await handler.ExecuteCommandAsync(context, provider);

            // Due to async nature of InteractionFramework, the result here may always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.ConvertFailed:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    case InteractionCommandError.ParseFailed:
                        break;
                    case null:
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
    
    private static Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    break;
                case InteractionCommandError.UnknownCommand:
                    break;
                case InteractionCommandError.ConvertFailed:
                    break;
                case InteractionCommandError.BadArgs:
                    break;
                case InteractionCommandError.Exception:
                    break;
                case InteractionCommandError.Unsuccessful:
                    break;
                case InteractionCommandError.ParseFailed:
                    break;
                case null:
                    break;
                default:
                    break;
            }

        return Task.CompletedTask;
    }
}