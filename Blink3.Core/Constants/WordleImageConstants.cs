using SixLabors.ImageSharp;

namespace Blink3.Core.Constants;

public static class WordleImageConstants
{
    public static readonly Color BackgroundColour = Color.FromRgb(19, 19, 19);
    public static readonly Color TextColour = Color.FromRgb(217, 220, 221);
    public static readonly Color CorrectTileColour = Color.FromRgb(45, 101, 44);
    public static readonly Color MisplacedTileColour = Color.FromRgb(211, 162, 64);
    public static readonly Color IncorrectTileColour = Color.FromRgb(43, 43, 43);

    public static readonly Color KeyUnavailableColour = Color.FromRgb(50, 50, 50); // Dark gray for unavailable
    public static readonly Color KeyAvailableColour = Color.FromRgb(96, 96, 96); // Subtle lighter gray for available
}