using Blink3.Core.Caching;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Blink3.Core.Services.Generators;

/// <summary>
///     Generates Wordle guess images.
/// </summary>
public class WordleGuessImageGenerator : IWordleGuessImageGenerator
{
    /// <summary>
    ///     Size of each tile in the generated image.
    /// </summary>
    private const int TileSize = 128;

    /// <summary>
    ///     Represents the font size used in the WordleGuessImageGenerator.
    /// </summary>
    private const int FontSize = 72;

    /// <summary>
    ///     The font size used for icons in the WordleGuessImageGenerator.
    /// </summary>
    /// <value>The font size used for icons.</value>
    private const int IconFontSize = 18;

    /// <summary>
    ///     The size of the margin used in the WordleGuessImageGenerator.
    /// </summary>
    /// <remarks>
    ///     This value represents the distance between the edge of the generated image and the letter tiles.
    ///     It is used to create a visually pleasing layout with proper spacing between the letters and the edge of the image.
    ///     The margin size is defined in pixels.
    /// </remarks>
    private const int MarginSize = 5;

    /// <summary>
    ///     Represents the size of a letter in the WordleGuessImageGenerator class.
    /// </summary>
    private const int LetterSize = TileSize - 2 * MarginSize;

    /// <summary>
    ///     Represents the height of the image generated by the WordleGuessImageGenerator class.
    ///     The height is calculated based on the size of the tiles and the margin.
    /// </summary>
    /// <remarks>
    ///     The ImageHeight is calculated by adding twice the MarginSize to the TileSize.
    /// </remarks>
    private const int ImageHeight = TileSize + 2 * MarginSize;

    /// <summary>
    ///     The Y-coordinate of the rectangle used to draw a letter tile in the WordleGuessImageGenerator class.
    /// </summary>
    private const int RectY = 2 * MarginSize;

    /// <summary>
    ///     The Y-coordinate position of the icon within the tile.
    /// </summary>
    private const float IconY = RectY + MarginSize;

    /// <summary>
    ///     The font used for generating Wordle guess images.
    /// </summary>
    private readonly Font _font;

    /// <summary>
    ///     Represents the icon font used in generating Wordle guess images.
    /// </summary>
    private readonly Font _iconFont;

    /// <summary>
    ///     Generates an image representing a WordleGuess object asynchronously.
    /// </summary>
    public WordleGuessImageGenerator(ICachingService cachingService, ILogger<WordleGuessImageGenerator> logger)
    {
        FontCollection fontCollection = new();
        string fontsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Fonts");
        FontFamily fontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Geologica.ttf"));
        _font = fontFamily.CreateFont(FontSize);
        FontFamily iconFontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Icons.ttf"));
        _iconFont = iconFontFamily.CreateFont(IconFontSize);
    }

    /// <summary>
    ///     Generates an image representing a WordleGuess object asynchronously.
    /// </summary>
    /// <param name="guess">The WordleGuess object to generate an image for.</param>
    /// <param name="options">The options for generating the image.</param>
    /// <param name="outStream">The memory stream to save the created image to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public async Task GenerateImageAsync(WordleGuess guess,
        WordleGuessImageGeneratorOptions options,
        MemoryStream outStream,
        CancellationToken cancellationToken = default)
    {
        // Create a new image and save it to the memory stream
        await CreateAndSaveImageAsync(guess, options, outStream, cancellationToken);
    }

    /// <summary>
    ///     Creates and saves an image representing a Wordle guess asynchronously.
    /// </summary>
    /// <param name="guess">The Wordle guess.</param>
    /// <param name="options">The image generator options.</param>
    /// <param name="outStream">The output memory stream where the image will be saved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateAndSaveImageAsync(WordleGuess guess,
        WordleGuessImageGeneratorOptions options,
        MemoryStream outStream,
        CancellationToken cancellationToken)
    {
        // Calculate image width
        int imageWidth = TileSize * guess.Letters.Count + 2 * MarginSize;

        // Create new image
        using Image<Rgba32> image = new(imageWidth, ImageHeight);
        TextOptions textOptions = new(_font)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = LetterSize
        };

        guess.Letters = guess.Letters.OrderBy(p => p.Position).ToList();
        // Fill background and draw letters
        image.Mutate(im =>
        {
            im.Fill(options.BackgroundColour);
            for (int i = 0; i < guess.Letters.Count; i++)
                DrawLetterAndIcon(im, guess.Letters[i], i, options, _font, _iconFont, LetterSize, textOptions);
        });

        // Save image to memory stream
        await image.SaveAsync(outStream, new PngEncoder(), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Draws a letter and an icon on the image using the specified parameters.
    /// </summary>
    /// <param name="im">The image processing context.</param>
    /// <param name="letter">The WordleLetter object representing the letter to be drawn.</param>
    /// <param name="index">The index of the letter.</param>
    /// <param name="options">The WordleGuessImageGeneratorOptions object containing the drawing options.</param>
    /// <param name="font">The font used for drawing the letter.</param>
    /// <param name="iconFont">The font used for drawing the icon.</param>
    /// <param name="letterSize">The size of the letter tile.</param>
    /// <param name="textOptions">The text options used for drawing the text.</param>
    private static void DrawLetterAndIcon(IImageProcessingContext im,
        WordleLetter letter,
        int index,
        WordleGuessImageGeneratorOptions options,
        Font font,
        Font iconFont,
        int letterSize,
        TextOptions textOptions)
    {
        // Draw and fill letter tile
        int rectX = index * TileSize + 2 * MarginSize;
        Color tileColour = letter.State switch
        {
            WordleLetterStateEnum.Correct => options.CorrectTileColour,
            WordleLetterStateEnum.Misplaced => options.MisplacedTileColour,
            _ => options.IncorrectTileColour
        };
        im.Fill(tileColour, new Rectangle(rectX, RectY, letterSize, letterSize));

        // Measure the text size, write letter in center of tile
        string text = letter.Letter.ToString().ToUpper();
        FontRectangle textSize = TextMeasurer.MeasureAdvance(text, textOptions);
        float textX = rectX + (letterSize - textSize.Width) / 2;
        float textY = RectY + (letterSize - textSize.Height) / 2;
        im.DrawText(text, font, options.TextColour, new PointF(textX, textY));

        // Draw tile icon
        string icon = letter.State.GetIcon().ToString();
        FontRectangle iconSize = TextMeasurer.MeasureBounds(icon, new TextOptions(iconFont));
        float iconX = rectX + letterSize - iconSize.Width - MarginSize;
        im.DrawText(icon, iconFont, options.TextColour, new PointF(iconX, IconY));
    }
}