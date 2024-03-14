namespace Blink3.Bot.MessageStyles.Variations;

/// <summary>
/// The <see cref="InfoStyle"/> class represents a message style for informational operations.
/// </summary>
public class InfoStyle : EmbedStyle
{
    public override void Apply(string? name = null)
    {
        Name = name ?? "Info";
        IconUrl = Icons.Info;
        Color = Colours.Info;
    }
}