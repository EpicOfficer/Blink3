using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Blink3.Core.Services.Generators;

/// <summary>
///     This class provides methods to generate an image representing a Wordle guess.
/// </summary>
public static class WordleGuessImageGenerator
{
    /// <summary>
    ///     Generates an image representing a WordleGuess object asynchronously.
    /// </summary>
    /// <param name="guess">The WordleGuess object to generate an image for.</param>
    /// <param name="options">The options for generating the image.</param>
    /// <param name="outStream">The memory stream to save created image to</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public static async Task GenerateImageAsync(WordleGuess guess,
        WordleGuessImageGeneratorOptions options,
        MemoryStream outStream,
        CancellationToken cancellationToken = default)
    {
        // Calculate dimensions for image
        int letterSize = options.TileSize - 2 * options.MarginSize;
        int imageHeight = options.TileSize + 2 * options.MarginSize;
        int imageWidth = options.TileSize * guess.Letters.Count + 2 * options.MarginSize;

        // Prepare fonts
        string fontsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Fonts");
        FontCollection fontCollection = new();
        FontFamily fontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Geologica.ttf"));
        Font font = fontFamily.CreateFont(options.FontSize);
        FontFamily iconFontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Icons.ttf"));
        Font iconFont = iconFontFamily.CreateFont(options.IconFontSize);

        // Create new image
        using Image<Rgba32> image = new(imageWidth, imageHeight);
        TextOptions textOptions = new(font)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = letterSize
        };

        // Fill background and draw letters
        image.Mutate(im =>
        {
            im.Fill(options.BackgroundColour);
            for (int i = 0; i < guess.Letters.Count; i++)
                DrawLetterAndIcon(im, guess.Letters[i], i, options, font, iconFont, letterSize, textOptions);
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
        int rectX = index * options.TileSize + 2 * options.MarginSize;
        int rectY = 2 * options.MarginSize;
        Color tileColour = letter.State switch
        {
            WordleLetterStateEnum.Correct => options.CorrectTileColour,
            WordleLetterStateEnum.Misplaced => options.MisplacedTileColour,
            _ => options.IncorrectTileColour
        };
        im.Fill(tileColour, new Rectangle(rectX, rectY, letterSize, letterSize));

        // Measure the text size, write letter in center of tile
        string text = letter.Letter.ToString().ToUpper();
        FontRectangle textSize = TextMeasurer.MeasureAdvance(text, textOptions);
        float textX = rectX + (letterSize - textSize.Width) / 2;
        float textY = rectY + (letterSize - textSize.Height) / 2;
        im.DrawText(text, font, options.TextColour, new PointF(textX, textY));

        // Draw tile icon
        string icon = letter.State.GetIcon().ToString();
        FontRectangle iconSize = TextMeasurer.MeasureBounds(icon, new TextOptions(iconFont));
        float iconX = rectX + letterSize - iconSize.Width - options.MarginSize;
        float iconY = rectY + options.MarginSize;
        im.DrawText(icon, iconFont, options.TextColour, new PointF(iconX, iconY));
    }
}