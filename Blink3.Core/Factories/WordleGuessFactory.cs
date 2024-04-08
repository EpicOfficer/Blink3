using Blink3.Core.Entities;
using Blink3.Core.Enums;

namespace Blink3.Core.Factories;

/// <summary>
///     Represents a factory class for creating WordleGuess objects.
/// </summary>
public static class WordleGuessFactory
{
    /// <summary>
    ///     Creates a new WordleGuess object based on the provided parameters.
    /// </summary>
    /// <param name="wordle">The Wordle game to associate the guess with.</param>
    /// <param name="word">The word entered by the player as a guess.</param>
    /// <param name="userId">The unique identifier of the player making the guess.</param>
    /// <returns>A new WordleGuess object.</returns>
    public static WordleGuess Create(Wordle wordle, string word, ulong userId)
    {
        return new WordleGuess
        {
            WordleId = wordle.Id,
            GuessedById = userId,
            Letters = Enumerable.Range(0, word.Length)
                .Select(i => new WordleLetter
                    { Position = i, Letter = word[i], State = WordleLetterStateEnum.Incorrect })
                .ToList()
        };
    }
}