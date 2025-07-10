using System.Diagnostics.CodeAnalysis;
using Blink3.Core.Caching;
using Blink3.Core.Entities;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable SpecifyStringComparison

namespace Blink3.DataAccess.Repositories;

public class WordRepository(BlinkDbContext dbContext, ICachingService cache) : IWordRepository
{
    [SuppressMessage("Performance",
        "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    public async Task<bool> IsGuessableAsync(string word, string lang,
        CancellationToken cancellationToken = new())
    {
        HashSet<string> wordList = await LoadWordListAsync(lang, cancellationToken);
        return wordList.Contains(word);
    }
    
    private async Task<HashSet<string>> LoadWordListAsync(string lang, CancellationToken cancellationToken)
    {
        string cacheKey = $"WordList_{lang}";

        return await cache.GetOrAddAsync(cacheKey, async () =>
        {
            List<string> words = await dbContext.Words
                .AsNoTracking()
                .Where(w => w.Language == lang)
                .Select(w => w.Text)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            
            return new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
        }, TimeSpan.FromHours(12), cancellationToken);
    }
    
    public async Task<Dictionary<WordKey, Word>> GetAllAsync(CancellationToken cancellationToken = new())
    {
        List<Word> allWords = await dbContext.Words
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Dictionary<WordKey, Word> uniqueWords = new();
        foreach (Word word in allWords)
        {
            WordKey key = new(word.Language, word.Text);
            uniqueWords.TryAdd(key, word);
        }

        return uniqueWords;
    }

    public async Task BulkAddAsync(IEnumerable<Word> newWords, CancellationToken cancellationToken = new())
    {
        await dbContext.BulkInsertAsync(newWords, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkUpdateAsync(IEnumerable<Word> updateWords, CancellationToken cancellationToken = new())
    {
        await dbContext.BulkUpdateAsync(updateWords, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetRandomSolutionAsync(string lang = "en", int length = 5,
        CancellationToken cancellationToken = new())
    {
        return await dbContext.Words
            .AsNoTracking()
            .Where(w => w.Language == lang && w.IsSolution && w.Length == length)
            .OrderBy(_ => EF.Functions.Random())
            .Select(w => w.Text)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}