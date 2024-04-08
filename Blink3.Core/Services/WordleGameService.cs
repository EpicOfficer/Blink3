using Blink3.Core.Configuration;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Factories;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace Blink3.Core.Services;

/// <summary>
///     Represents a service for playing the Wordle game.
/// </summary>
public class WordleGameService(
    IWordleRepository wordleRepository,
    IOptions<BlinkConfiguration> config) : IWordleGameService
{
    public async Task<Result<WordleGuess>> MakeGuessAsync(string word, ulong userId, Wordle wordle)
    {
        if (wordle.ValidateWordLength(word) is not true)
            return Result<WordleGuess>.Fail($"The word you guessed does not match the wordle length {wordle.WordToGuess.Length}");
        
        WordleGuess? oldGuess = wordle.Guesses.FirstOrDefault(w => w.Word == word);
        if (oldGuess is not null) return Result<WordleGuess>.Ok(oldGuess);
    
        WordleGuess guess = WordleGuessFactory.Create(wordle, word, userId);

        wordle.ProcessGuess(guess);
        
        await wordleRepository.AddGuessAsync(wordle, guess);
        return Result<WordleGuess>.Ok(guess);
    }

    public async Task<MemoryStream> GenerateImageAsync(WordleGuess guess)
    {
        string fontsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Fonts");

        const int tileSize = 120;
        const int fontSize = 72;
        const int iconFontSize = 18;
        const int marginSize = 5;
        const int letterSize = tileSize - 2 * marginSize;
        const int imageHeight = tileSize + 2 * marginSize;
        Color textColor = Color.FromRgb(217,220,221);
        Color backGroundColor = Color.FromRgb(19, 19, 19);
        int imageWidth = tileSize * guess.Letters.Count + 2 * marginSize;
        
        FontCollection fontCollection = new();
        FontFamily fontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Geologica.ttf"));
        Font font = fontFamily.CreateFont(fontSize);

        FontFamily iconFontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Icons.ttf"));
        Font iconFont = iconFontFamily.CreateFont(iconFontSize);
        
        using Image<Rgba32> image = new(imageWidth, imageHeight);
        TextOptions options = new(font)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = letterSize
        };
        
        image.Mutate(im =>
        {
            im.Fill(backGroundColor);
            
            for (int i = 0; i < guess.Letters.Count; i++)
            {
                WordleLetter letter = guess.Letters[i];
                string text = letter.Letter.ToString().ToUpper();
                
                int rectX = i * tileSize + 2 * marginSize;
                const int rectY = 2 * marginSize;

                im.Fill(GetColorForLetter(letter.State), new
                    Rectangle(rectX, rectY, letterSize, letterSize));
                
                // Measure the text size.
                FontRectangle textSize = TextMeasurer.MeasureAdvance(text, options);
                
                float textX = rectX + (letterSize - textSize.Width) / 2;
                float textY = rectY + (letterSize - textSize.Height) / 2;
                
                im.DrawText(
                    text,
                    font,
                    textColor,
                    new PointF(textX, textY));

                string icon = GetIconForLetter(letter.State).ToString();
                FontRectangle iconSize = TextMeasurer.MeasureBounds(icon, new TextOptions(iconFont));

                float iconX = rectX + letterSize - iconSize.Width - marginSize;
                const float iconY = rectY + marginSize;
                
                im.DrawText(
                    icon,
                    iconFont,
                    textColor,
                    new PointF(iconX, iconY));
            }
        });

        MemoryStream memoryStream = new();
        await image.SaveAsync(memoryStream, new PngEncoder());

        return memoryStream;
    }
    
    private static Color GetColorForLetter(WordleLetterStateEnum state)
    {
        return state switch
        {
            WordleLetterStateEnum.Correct => Color.FromRgb(45,101,44),
            WordleLetterStateEnum.Misplaced => Color.FromRgb(211,162,64),
            _ => Color.FromRgb(43,43,43)
        };
    }
    
    private static char GetIconForLetter(WordleLetterStateEnum state)
    {
        return state switch
        {
            WordleLetterStateEnum.Correct => '\uE002',
            WordleLetterStateEnum.Misplaced => '\uE001',
            _ => '\uE000'
        };
    }
}