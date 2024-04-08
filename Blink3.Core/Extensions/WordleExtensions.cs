using Blink3.Core.Entities;

namespace Blink3.Core.Extensions;

/// <summary>
///     Contains extension methods for the Wordle class.
/// </summary>
public static class WordleExtensions
{
    /// <summary>
    ///     Processes a word guess in the Wordle game.
    /// </summary>
    /// <param name="wordle">The Wordle game instance.</param>
    /// <param name="guess">The word guess entered by a player.</param>
    public static void ProcessGuess(this Wordle wordle, WordleGuess guess)
    {
        List<int> correctIndices = [];
        List<int> misplacedIndices = [];
        guess.MarkCorrectLetters(wordle, correctIndices);
        guess.MarkMisplacedLetters(wordle, correctIndices, misplacedIndices);
    }

    /// <summary>
    ///     Validates the length of a given word against the length of the word to guess in a Wordle game.
    /// </summary>
    /// <param name="wordle">The Wordle game object.</param>
    /// <param name="guess">The word to check.</param>
    /// <returns>True if the length of the guess matches the length of the word to guess in the Wordle game, otherwise false.</returns>
    public static bool ValidateWordLength(this Wordle wordle, string guess)
    {
        return guess.Length == wordle.WordToGuess.Length;
    }
}