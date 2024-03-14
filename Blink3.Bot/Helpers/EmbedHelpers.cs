using Blink3.Bot.MessageStyles;
using Blink3.Bot.MessageStyles.Extensions;
using Blink3.Bot.MessageStyles.Variations;
using Discord;

namespace Blink3.Bot.Helpers;

public static class EmbedHelpers
{
    /// <summary>
    /// Create a basic embed with an <see cref="EmbedStyle"/>
    /// </summary>
    /// <param name="style">The <see cref="EmbedStyle"/> to set</param>
    /// <param name="name">The <see cref="EmbedStyle.Name"/>, otherwise style name if null.</param>
    /// <param name="message">The message to include in the embed</param>
    /// <param name="fields">Optional fields to include in the embed</param>
    /// <returns></returns>
    private static Embed CreateEmbed(EmbedStyle style, string? name = null, string message = "", EmbedFieldBuilder[]? fields = null)
    {
        return new EmbedBuilder()
            .WithStyle(style, name)
            .WithDescription(message)
            .WithFields(fields)
            .Build();
    }

    /// <summary>
    /// Create a simple plain message embed
    /// </summary>
    /// <param name="name">An optional title for the embed.</param>
    /// <param name="message">The message for the embed</param>
    /// <param name="fields">Optional fields to include in the embed</param>
    public static Embed CreatePlain(string? name = null, string message = "", EmbedFieldBuilder[]? fields = null)
        => CreateEmbed(new PlainStyle(), name, message, fields);
    
    /// <summary>
    /// Create a simple success message embed
    /// </summary>
    /// <param name="name">An optional title for the embed.  Defaults to 'Success'</param>
    /// <param name="message">The message for the embed</param>
    /// <param name="fields">Optional fields to include in the embed</param>
    public static Embed CreateSuccess(string? name = null, string message = "", EmbedFieldBuilder[]? fields = null)
        => CreateEmbed(new SuccessStyle(), name, message, fields);
    
    /// <summary>
    /// Create a simple info message embed
    /// </summary>
    /// <param name="message">The message for the embed</param>
    /// <param name="name">An optional title for the embed.  Defaults to 'Info'</param>
    /// <param name="fields">Optional fields to include in the embed</param>
    public static Embed CreateInfo(string? name = null, string message = "", EmbedFieldBuilder[]? fields = null)
        => CreateEmbed(new InfoStyle(), name, message, fields);
    
    /// <summary>
    /// Create a simple error message embed
    /// </summary>
    /// <param name="name">An optional title for the embed.  Defaults to 'Error'</param>
    /// <param name="message">The message for the embed</param>
    /// <param name="fields">Optional fields to include in the embed</param>
    public static Embed CreateError(string? name = null, string message = "", EmbedFieldBuilder[]? fields = null)
        => CreateEmbed(new ErrorStyle(), name, message, fields);
}