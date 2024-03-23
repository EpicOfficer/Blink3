namespace Blink3.Bot.MessageStyles.Variations;

/// <summary>
///     The <see cref="SuccessStyle" /> class represents a message style for successful operations.
/// </summary>
public class SuccessStyle : EmbedStyle
{
    public override void Apply(string? name = null)
    {
        Name = name ?? "Success";
        IconUrl = Icons.Success;
        Color = Colours.Success;
    }
}