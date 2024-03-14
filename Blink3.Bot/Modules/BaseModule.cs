using Blink3.Bot.Helpers;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

public class BlinkModuleBase<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
    /// <summary>
    /// Respond with a plain message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected virtual async Task RespondPlainAsync(string? name = null, string message = "", bool ephemeral = true, MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
        => await RespondAsync("", embed: EmbedHelpers.CreatePlain(name, message, embedFields), ephemeral: ephemeral, components: components);
    
    /// <summary>
    /// Respond with a success message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Success'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected virtual async Task RespondSuccessAsync(string? name = null, string message = "", bool ephemeral = true, MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
        => await RespondAsync("", embed: EmbedHelpers.CreateSuccess(name, message, embedFields), ephemeral: ephemeral, components: components);

    /// <summary>
    /// Respond with an info message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Info'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected virtual async Task RespondInfoAsync(string? name = null, string message = "", bool ephemeral = true, MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
        => await RespondAsync("", embed: EmbedHelpers.CreateInfo(name, message, embedFields), ephemeral: ephemeral, components: components);

    /// <summary>
    /// Respond with an error message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Error'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected virtual async Task RespondErrorAsync(string? name = null, string message = "", bool ephemeral = true, MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
        => await RespondAsync("", embed: EmbedHelpers.CreateError(name, message, embedFields), ephemeral: ephemeral, components: components);
}