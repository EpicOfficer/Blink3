using Discord;

// ReSharper disable UnusedMember.Global

namespace Blink3.Bot.MessageStyles;

/// <summary>
///     The Colours class provides constant color values for different styles.
/// </summary>
public static class Colours
{
    /// <summary>
    ///     The primary color used in Discord messages.
    /// </summary>
    public static readonly Color Primary = new(255, 187, 71);

    /// <summary>
    ///     Represents the success color used in message styles.
    /// </summary>
    public static readonly Color Success = new(43, 182, 115);

    /// <summary>
    ///     Represents a constant color value for representing danger in message styles.
    /// </summary>
    public static readonly Color Danger = new(231, 76, 60);

    /// <summary>
    ///     The Info color used in message styles.
    /// </summary>
    public static readonly Color Info = new(33, 150, 243);
}