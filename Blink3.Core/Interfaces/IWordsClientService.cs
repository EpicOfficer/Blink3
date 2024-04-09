using Blink3.Core.Models;

namespace Blink3.Core.Interfaces;

/// <summary>
///     Represents a service for retrieving word definitions.
/// </summary>
public interface IWordsClientService
{
    /// <summary>
    ///     Retrieves the definition of a word asynchronously.
    /// </summary>
    /// <param name="word">The word to get the definition of.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    ///     The task result contains a WordDetails object that represents the definition of the word.
    ///     If the word is not found, the task result will be null.
    /// </returns>
    Task<WordDetails?> GetDefinitionAsync(string word, CancellationToken cancellationToken = default);
}