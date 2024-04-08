using SixLabors.ImageSharp;

namespace Blink3.Core.Services.Generators;

/// <summary>
///     Represents the options for generating a Wordle guess image.
/// </summary>
public class WordleGuessImageGeneratorOptions
{
    /// <summary>
    ///     Gets or sets the size of the tiles.
    /// </summary>
    /// <remarks>
    ///     This property represents the size of the tiles in the WordleGuessImageGeneratorOptions class.
    /// </remarks>
    public int TileSize { get; set; }

    /// <summary>
    ///     The font size used for text in WordleGuessImageGeneratorOptions.
    /// </summary>
    public int FontSize { get; set; }

    /// <summary>
    ///     Gets or sets the font size for the icons.
    /// </summary>
    /// <value>
    ///     The font size for the icons.
    /// </value>
    public int IconFontSize { get; set; }

    /// <summary>
    ///     Gets or sets the margin size for the WordleGuessImageGeneratorOptions.
    /// </summary>
    public int MarginSize { get; set; }

    /// <summary>
    ///     Gets or sets the color of the text.
    /// </summary>
    /// <value>
    ///     The color of the text.
    /// </value>
    public Color TextColour { get; set; }

    /// <summary>
    ///     Gets or sets the background colour used for generating the wordle guess image.
    /// </summary>
    public Color BackgroundColour { get; set; }

    /// <summary>
    ///     Gets or sets the color of the correct tile in the Wordle guess image generator options.
    /// </summary>
    /// <value>
    ///     The color of the correct tile.
    /// </value>
    public Color CorrectTileColour { get; set; }

    /// <summary>
    ///     The color of the misplaced tile in a Wordle guess image generator.
    /// </summary>
    public Color MisplacedTileColour { get; set; }

    /// <summary>
    ///     Gets or sets the color of the incorrect tile.
    /// </summary>
    /// <value>
    ///     The color of the incorrect tile.
    /// </value>
    public Color IncorrectTileColour { get; set; }
}