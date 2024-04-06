using Blink3.Core.Entities;

namespace Blink3.Core.Interfaces;

public interface IWordleGameService
{
    /// <summary>
    ///     Makes a guess in the Wordle game.
    /// </summary>
    /// <param name="word">The word that the player is guessing.</param>
    /// <param name="userId">The ID of the user making the guess.</param>
    /// <param name="wordle">The Wordle game.</param>
    /// <returns>The WordleGuess object representing the player's guess.</returns>
    public Task<WordleGuess> MakeGuessAsync(string word, ulong userId, Wordle wordle);
}