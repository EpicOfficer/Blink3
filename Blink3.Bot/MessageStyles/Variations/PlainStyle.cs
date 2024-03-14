namespace Blink3.Bot.MessageStyles.Variations;

/// <summary>
/// The <see cref="PlainStyle"/> class represents a message style for informational operations with no icon.
/// </summary>
public class PlainStyle : EmbedStyle
{
    public override void Apply(string? name = null)
    {
        Name = name ?? "_ _";
        Color = Colours.Info;
    }
}