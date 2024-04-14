using Blink3.Core.Configuration;
using Blink3.Core.Entities;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blink3.DataAccess.Services;

/// <summary>
///     Represents a service for seeding words from a text files into the database.
/// </summary>
public class WordSeedService(
    IServiceScopeFactory scopeFactory,
    IOptions<BlinkConfiguration> config,
    ILogger<WordSeedService> logger) : IHostedService
{
    private BlinkConfiguration Config => config.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting word seed service, this could take a while...");
        using IServiceScope scope = scopeFactory.CreateScope();
        IWordRepository words = scope.ServiceProvider.GetRequiredService<IWordRepository>();

        Dictionary<WordKey, Word> existingWords = await words.GetAllAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Got {count} existing words from database", existingWords.Count);

        List<Word> newWords = [];
        List<Word> wordsToUpdate = [];

        Dictionary<string, WordListConfig> wordLists = Config.WordLists;
        foreach ((string language, WordListConfig files) in wordLists)
        {
            string solutionWordsFile = files.SolutionWordsFile;
            string? guessWordsFile = files.GuessWordsFile;

            logger.LogInformation("Reading solution words for language '{lang}' from file '{file}'...",
                language, solutionWordsFile);
            List<Word> solutionWords = await GetWordsFromFile(
                solutionWordsFile,
                true,
                language,
                cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Got {count} solution words for language '{lang}'",
                solutionWords.Count,
                language);

            List<Word> guessWords = [];
            if (guessWordsFile is not null)
            {
                logger.LogInformation("Reading guess words for language '{lang}' from file '{file}'...",
                    language, guessWordsFile);
                 guessWords = await GetWordsFromFile(
                     guessWordsFile,
                    false,
                    language,
                    cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Got {count} guess words for language '{lang}'",
                    guessWords.Count,
                    language);
            }
            
            foreach (Word newWord in solutionWords.Concat(guessWords))
                if (existingWords.TryGetValue(new WordKey(newWord.Language, newWord.Text), out Word? existingWord))
                {
                    // Word exists in this language
                    if (existingWord.IsSolution == newWord.IsSolution) continue;

                    existingWord.IsSolution = newWord.IsSolution;
                    wordsToUpdate.Add(existingWord);
                }
                else
                {
                    // New word in this language
                    newWords.Add(newWord);
                }
        }

        logger.LogInformation("Adding {count} new words to database...", newWords.Count);
        await words.BulkAddAsync(newWords, cancellationToken).ConfigureAwait(false);
        
        logger.LogInformation("Updating {count} existing words in database...", wordsToUpdate.Count);
        await words.BulkUpdateAsync(wordsToUpdate, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Word seed service complete!");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Retrieves a list of words from a file.
    /// </summary>
    /// <param name="wordsFile">The path to the file containing the words.</param>
    /// <param name="isSolution">Specifies whether the words are solution words.</param>
    /// <param name="language">The language of the words.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A list of Word objects read from the file.</returns>
    private static async Task<List<Word>> GetWordsFromFile(string wordsFile, bool isSolution, string language,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(wordsFile)) return [];
        
        return (await File.ReadAllLinesAsync(wordsFile, cancellationToken).ConfigureAwait(false))
            .Select(word => new Word
            {
                Text = word,
                IsSolution = isSolution,
                Language = language
            }).ToList();
    }
}