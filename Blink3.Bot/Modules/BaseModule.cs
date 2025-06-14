using Blink3.Bot.MessageStyles.Helpers;
using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

public class BlinkModuleBase<T>(IUnitOfWork? unitOfWork = null)
    : InteractionModuleBase<T> where T : class, IInteractionContext
{
    /// <summary>
    ///     Responds to an interaction with the provided embed, ephemeral flag, and message components.
    ///     If the interaction has already been responded to, a follow-up message is sent instead.
    /// </summary>
    /// <param name="embed">The embed to include in the response</param>
    /// <param name="ephemeral">Whether the response should be ephemeral</param>
    /// <param name="components">Optional message components to include</param>
    /// <returns>A task representing the asynchronous operation</returns>
    protected Task RespondOrFollowUpAsync(Embed? embed = null, bool ephemeral = false,
        MessageComponent? components = null, AllowedMentions? allowedMentions = null)
    {
        return Context.Interaction.HasResponded
            ? FollowupAsync(embed: embed, ephemeral: ephemeral, components: components,
                allowedMentions: allowedMentions)
            : RespondAsync(embed: embed, ephemeral: ephemeral, components: components,
                allowedMentions: allowedMentions);
    }

    /// <summary>
    ///     Respond with a plain message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected async Task RespondPlainAsync(string? name = null, string message = "", bool ephemeral = true,
        MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
    {
        await RespondOrFollowUpAsync(EmbedHelpers.CreatePlain(name, message, embedFields), ephemeral, components);
    }

    /// <summary>
    ///     Respond with a success message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Success'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected async Task RespondSuccessAsync(string? name = null, string message = "", bool ephemeral = true,
        MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
    {
        await RespondOrFollowUpAsync(EmbedHelpers.CreateSuccess(name, message, embedFields), ephemeral, components);
    }

    /// <summary>
    ///     Respond with an info message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Info'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected async Task RespondInfoAsync(string? name = null, string message = "", bool ephemeral = true,
        MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
    {
        await RespondOrFollowUpAsync(EmbedHelpers.CreateInfo(name, message, embedFields), ephemeral, components);
    }

    /// <summary>
    ///     Respond with an error message
    /// </summary>
    /// <param name="message">Message to send to the user</param>
    /// <param name="name">Embed title, default to 'Error'</param>
    /// <param name="ephemeral">Whether the message should be ephemeral, defaults to true.</param>
    /// <param name="components">Optional message components to include</param>
    /// <param name="embedFields">Optional fields to include in the embed</param>
    protected async Task RespondErrorAsync(string? name = null, string message = "", bool ephemeral = true,
        MessageComponent? components = null, EmbedFieldBuilder[]? embedFields = null)
    {
        await RespondOrFollowUpAsync(EmbedHelpers.CreateError(name, message, embedFields), ephemeral, components);
    }

    protected async Task<BlinkGuild> FetchConfig()
    {
        if (unitOfWork is not null && Context.Interaction.GuildId is not null)
            return await unitOfWork.BlinkGuildRepository.GetOrCreateByIdAsync(Context.Interaction.GuildId);

        return new BlinkGuild();
    }
}