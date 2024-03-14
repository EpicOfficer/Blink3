using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

public class BlinkModuleBase<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
    protected async Task RespondSuccessAsync(string text)
    {
        await RespondAsync(text);
    }
}