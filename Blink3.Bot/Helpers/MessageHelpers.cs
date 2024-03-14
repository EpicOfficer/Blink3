using Blink3.Bot.MessageStyles;
using Blink3.Bot.MessageStyles.Extensions;
using Blink3.Bot.MessageStyles.Variations;
using Discord;

namespace Blink3.Bot.Helpers;

public static class MessageHelpers
{
    /// <summary>
    /// Create a basic embed with an <see cref="EmbedStyle"/>
    /// </summary>
    /// <param name="style">The <see cref="EmbedStyle"/> to set</param>
    /// <param name="message">The message to include in the embed</param>
    /// <param name="name">The <see cref="EmbedStyle.Name"/>, otherwise style name if null.</param>
    /// <returns></returns>
    private static Embed CreateEmbed(EmbedStyle style, string message, string? name = null)
    {
        return new EmbedBuilder()
            .WithStyle(style, name)
            .WithDescription(message)
            .Build();
    }

    /// <summary>
    /// Create a simple plain message embed
    /// </summary>
    /// <param name="message">The message for the embed</param>
    /// <param name="name">An optional title for the embed.</param>
    /// <returns></returns>
    public static Embed CreatePlain(string message, string? name = null) => CreateEmbed(new PlainStyle(), message, name);

    
    /// <summary>
    /// Create a simple success message embed
    /// </summary>
    /// <param name="message">The message for the embed</param>
    /// <param name="name">An optional title for the embed.  Defaults to 'Success'</param>
    /// <returns></returns>
    public static Embed CreateSuccess(string message, string? name = null) => CreateEmbed(new SuccessStyle(), message, name);
    
    /// <summary>
    /// Create a simple info message embed
    /// </summary>
    /// <param name="message">The message for the embed</param>
    /// <param name="name">An optional title for the embed.  Defaults to 'Info'</param>
    /// <returns></returns>
    public static Embed CreateInfo(string message, string? name = null) => CreateEmbed(new InfoStyle(), message, name);
    
    /// <summary>
    /// Create a simple error message embed
    /// </summary>
    /// <param name="message">The message for the embed</param>
    /// <param name="name">An optional title for the embed.  Defaults to 'Error'</param>
    public static Embed CreateError(string message, string? name = null) => CreateEmbed(new ErrorStyle(), message, name);
}