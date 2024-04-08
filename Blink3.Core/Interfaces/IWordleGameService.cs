using Blink3.Core.Entities;
using Blink3.Core.Models;

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
    public Task<Result<WordleGuess>> MakeGuessAsync(string word, ulong userId, Wordle wordle);

    /// <summary>
    ///     Generates an image for the provided WordleGuess object.
    /// </summary>
    /// <param name="guess">The WordleGuess object for which to generate an image.</param>
    /// <returns>A Task representing the asynchronous operation that returns a MemoryStream containing the generated image.</returns>
    public Task<MemoryStream> GenerateImageAsync(WordleGuess guess);
}