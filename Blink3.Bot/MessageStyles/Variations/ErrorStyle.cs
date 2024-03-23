namespace Blink3.Bot.MessageStyles.Variations;

/// <summary>
///     The <see cref="ErrorStyle" /> class represents a message style for failed operations.
/// </summary>
public class ErrorStyle : EmbedStyle
{
    public override void Apply(string? name = null)
    {
        Name = name ?? "Error";
        IconUrl = Icons.Error;
        Color = Colours.Danger;
    }
}