using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating Wordle game data.
/// </summary>
public interface IWordleRepository : IBaseGameRepository<Wordle>
{
    /// <summary>
    ///     Adds a Wordle guess to the repository asynchronously.
    /// </summary>
    /// <param name="wordle">The Wordle game entity containing the guess to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddGuessAsync(Wordle wordle, WordleGuess guess, CancellationToken cancellationToken = default);
}