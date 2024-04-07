using Blink3.Core.Entities;
using Blink3.Core.Models;

namespace Blink3.Core.Repositories.Interfaces;

/// <summary>
///     Represents a repository for managing words.
/// </summary>
public interface IWordRepository
{
    /// <summary>
    ///     Determines whether a word is guessable in the specified language.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <param name="lang">The language in which to check the word. Defaults to "en".</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     <c>true</c> if the word is guessable in the specified language; otherwise, <c>false</c>.
    /// </returns>
    public Task<bool> IsGuessableAsync(string word, string lang = "en", CancellationToken cancellationToken = new());

    /// <summary>
    ///     Generates a random solution word of the specified length and language.
    /// </summary>
    /// <param name="length">The length of the solution word. Default value is 5.</param>
    /// <param name="lang">The language of the solution word. Default value is "en".</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A randomly generated solution.</returns>
    public Task<string> GetRandomSolutionAsync(int length = 5, string lang = "en",
        CancellationToken cancellationToken = new());

    /// <summary>
    ///     Retrieves all words from the word repository.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of WordKey and Word objects representing all words in the repository.</returns>
    public Task<Dictionary<WordKey, Word>> GetAllAsync(CancellationToken cancellationToken = new());

    /// <summary>
    ///     Adds multiple new words to the repository in bulk.
    /// </summary>
    /// <param name="newWords">The collection of new words to be added.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task BulkAddAsync(IEnumerable<Word> newWords, CancellationToken cancellationToken = new());

    /// <summary>
    ///     Performs a bulk update of words in the database.
    /// </summary>
    /// <param name="updateWords">The collection of words to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task BulkUpdateAsync(IEnumerable<Word> updateWords, CancellationToken cancellationToken = new());
}