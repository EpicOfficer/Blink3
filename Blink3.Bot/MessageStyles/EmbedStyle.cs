using Discord;

namespace Blink3.Bot.MessageStyles;

public abstract class EmbedStyle
{
    /// <summary>
    /// Gets or sets the name of the embed style.
    /// </summary>
    /// <remarks>
    /// The name identifies the type or variation of the embed style.
    /// </remarks>
    public string? Name { get; protected set; }

    /// <summary>
    /// Gets or sets the URL of the icon used in the embed style.
    /// </summary>
    public string? IconUrl { get; protected set; }

    /// <summary>
    /// Represents a color used in Discord message styles.
    /// </summary>
    public Color Color { get; protected set; }

    /// <summary>
    /// Applies the style to the embed.
    /// </summary>
    /// <param name="name">Optional name of the style. If provided, it will be set as the Name property of the style. Defaults to null.</param>
    public abstract void Apply(string? name = null);
}