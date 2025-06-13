using Blink3.Core.Entities;
using Blink3.Core.Models;

namespace Blink3.Core.Interfaces;

public interface IWordleGameService
{
    /// <summary>
    ///     Starts a new game in the Wordle game.
    /// </summary>
    /// <param name="channelId">The ID of the channel where the game will be played.</param>
    /// <param name="language">The language used in the game.</param>
    /// <param name="length">The length of the word to be guessed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation.
    ///     The task result contains a <see cref="Wordle" /> object representing the newly created Wordle game.
    /// </returns>
    public Task<Wordle> StartNewGameAsync(ulong channelId, string language, int length,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Determines whether a game is in progress for the specified channel.
    /// </summary>
    /// <param name="channelId">The ID of the channel to check.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    ///     A Task that represents the asynchronous operation. The task result contains a boolean value
    ///     indicating whether a game is in progress for the specified channel.
    /// </returns>
    public Task<bool> IsGameInProgressAsync(ulong channelId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Makes a guess in the Wordle game.
    /// </summary>
    /// <param name="word">The word that the player is guessing.</param>
    /// <param name="userId">The ID of the user making the guess.</param>
    /// <param name="wordle">The Wordle game.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The WordleGuess object representing the player's guess.</returns>
    public Task<Result<WordleGuess>> MakeGuessAsync(string word, ulong userId, Wordle wordle,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates an image for the provided WordleGuess object.
    /// </summary>
    /// <param name="guess">The WordleGuess object for which to generate an image.</param>
    /// <param name="outStream">The output MemoryStream where the generated image will be written to.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the asynchronous operation.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task GenerateImageAsync(WordleGuess guess,
        MemoryStream outStream,
        BlinkGuild blinkGuild,
        CancellationToken cancellationToken = default);
    
    public Task GenerateStatusImageAsync(Wordle wordle,
        MemoryStream outStream,
        BlinkGuild blinkGuild,
        CancellationToken cancellationToken = default);
}