using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for accessing and manipulating WordleGuess entities.
/// </summary>
public interface IWordleGuessRepository : IGenericRepository<WordleGuess>
{
    /// <summary>
    ///     Retrieves a WordleGuess object by the specified word from the given Wordle object.
    /// </summary>
    /// <param name="wordle">The Wordle object to search in.</param>
    /// <param name="word">The word to search for.</param>
    /// <returns>
    ///     The matching WordleGuess object if found, or null if no matching word is found.
    /// </returns>
    public WordleGuess? GetByWord(Wordle wordle, string word);
}