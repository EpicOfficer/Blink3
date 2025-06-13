using Blink3.Core.Entities;
using Blink3.Core.Services.Generators;

namespace Blink3.Core.Interfaces;

/// <summary>
///     Represents an interface for generating Wordle guess images.
/// </summary>
public interface IWordleGuessImageGenerator
{
    /// <summary>
    ///     Generates an image for a Wordle guess.
    /// </summary>
    /// <param name="guess">The Wordle guess containing the letters.</param>
    /// <param name="options">The options for generating the image.</param>
    /// <param name="outStream">The output stream to write the generated image to.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task GenerateImageAsync(WordleGuess guess,
        WordleGuessImageGeneratorOptions options,
        MemoryStream outStream,
        CancellationToken cancellationToken = default);

    public Task CreateAndSaveStatusImageAsync(Wordle wordle,
        MemoryStream outStream,
        CancellationToken cancellationToken = default);
}