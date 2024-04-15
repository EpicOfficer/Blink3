using Blink3.Core.Entities;
using Blink3.Core.Enums;

namespace Blink3.Tests.Helpers;

public static class WordleHelpers
{
    /// <summary>
    ///     Converts a string into an array of WordleLetter objects.
    /// </summary>
    /// <param name="word">The string to convert into WordleLetters.</param>
    /// <returns>An array of WordleLetter objects.</returns>
    public static List<WordleLetter> ConvertToWordleLetters(string word)
    {
        return word.ToCharArray().Select((c, i) => new WordleLetter
        {
            Position = i,
            Letter = c,
            State = WordleLetterStateEnum.Incorrect
        }).ToList();
    }

    /// <summary>
    ///     Creates a WordleGuess object based on the provided word.
    /// </summary>
    /// <param name="word">The word to create a WordleGuess for.</param>
    /// <returns>A WordleGuess object with the Letters property populated based on the provided word.</returns>
    public static WordleGuess CreateGuess(string word)
    {
        return new WordleGuess
        {
            Letters = ConvertToWordleLetters(word)
        };
    }
}