using Blink3.Core.Configuration;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;
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
    private BlinkConfiguration Config => config.Value;
    
    public async Task<WordleGuess> MakeGuessAsync(string word, ulong userId, Wordle wordle)
    {
        ValidateWordLength(word, wordle);

        WordleGuess? oldGuess = wordle.Guesses.FirstOrDefault(w => w.Word == word);
        if (oldGuess is not null) return oldGuess;
    
        WordleGuess guess = CreateInitialGuess(word, userId, wordle);

        List<int> correctIndices = [];
        List<int> misplacedIndices = [];
        MarkCorrectLetters(word, guess, wordle, correctIndices);
        MarkMisplacedLetters(word, guess, wordle, correctIndices, misplacedIndices);
        
        await wordleRepository.AddGuessAsync(wordle, guess);
        return guess;
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
    
    private static void ValidateWordLength(string word, Wordle wordle)
    {
        if (wordle.WordToGuess.Length != word.Length)
            throw new InvalidOperationException("Provided word is the wrong length for this wordle");
    }
    
    private static WordleGuess CreateInitialGuess(string word, ulong userId, Wordle wordle)
    {
        return new WordleGuess
        {
            WordleId = wordle.Id,
            GuessedById = userId,
            Letters = Enumerable.Range(0, word.Length)
                .Select(i => new WordleLetter { Position = i, Letter = word[i], State = WordleLetterStateEnum.Incorrect })
                .ToList()
        };
    }
    
    private static void MarkCorrectLetters(string word, WordleGuess guess, Wordle wordle, List<int> correctIndices)
    {
        string wordToGuess = wordle.WordToGuess;
        for(int i = 0; i < wordToGuess.Length; i++)
        {
            if (wordToGuess[i] != word[i]) continue;
            guess.Letters[i].State = WordleLetterStateEnum.Correct;
            correctIndices.Add(i);
        }
    }
    
    private static void MarkMisplacedLetters(string word, WordleGuess guess, Wordle wordle, List<int> correctIndices, List<int> misplacedIndices)
    {
        string wordToGuess = wordle.WordToGuess;
        for(int i = 0; i < word.Length; i++)
        {
            if (guess.Letters[i].State == WordleLetterStateEnum.Correct) continue;
            int index = wordToGuess.IndexOf(word[i]);
            if (index == -1 || correctIndices.Contains(index) || misplacedIndices.Contains(index)) continue;
            guess.Letters[i].State = WordleLetterStateEnum.Misplaced;
            misplacedIndices.Add(index);
        }
    }
    
    public async Task<MemoryStream> GenerateImageAsync(WordleGuess guess)
    {
        string fontsDirectory = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Fonts");

        const int tileSize = 120;
        const int fontSize = 72;
        const int marginSize = 5;
        const int letterSize = tileSize - 2 * marginSize;
        const int imageHeight = tileSize + 2 * marginSize;
        Color textColor = Color.FromRgb(217,220,221);
        Color backGroundColor = Color.FromRgb(19, 19, 19);
        int imageWidth = tileSize * guess.Letters.Count + 2 * marginSize;
        
        FontCollection fontCollection = new();
        FontFamily fontFamily = fontCollection.Add(Path.Join(fontsDirectory, "Geologica.ttf"));
        Font font = fontFamily.CreateFont(fontSize);
        
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
            }
        });

        MemoryStream memoryStream = new();
        await image.SaveAsync(memoryStream, new PngEncoder());

        return memoryStream;
    }
}