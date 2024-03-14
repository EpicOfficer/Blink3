using Discord;

namespace Blink3.Bot.MessageStyles.Extensions;

/// <summary>
/// Provides extension methods for the EmbedBuilder class.
/// </summary>
public static class EmbedBuilderExtensions
{
    /// <summary>
    /// Applies the specified style to the embed.
    /// </summary>
    /// <param name="builder">The <see cref="EmbedBuilder"/> to apply the style to.</param>
    /// <param name="style">The style to apply.</param>
    /// <param name="name">Optional name of the style. If provided, it will be set as the Name property of the style. Defaults to null.</param>
    /// <returns>The modified <see cref="EmbedBuilder"/> instance.</returns>
    public static EmbedBuilder WithStyle(this EmbedBuilder builder, EmbedStyle style, string? name = null)
    {
        style.Apply(name);

        builder.WithAuthor(author =>
        {
            author
                .WithName(style.Name)
                .WithIconUrl(style.IconUrl);
        })
        .WithColor(style.Color);

        return builder;
    }
}