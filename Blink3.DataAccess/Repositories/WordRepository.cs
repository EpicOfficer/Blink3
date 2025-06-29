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
    private const string SolutionCountCachePrefix = "Words_Solution_Count_";
    private readonly Random _random = new();
    
    private readonly Dictionary<string, HashSet<string>> _inMemoryCache = new(StringComparer.OrdinalIgnoreCase);

    [SuppressMessage("Performance",
        "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    public async Task<bool> IsGuessableAsync(string word, string lang,
        CancellationToken cancellationToken = new())
    {
        if (!_inMemoryCache.TryGetValue(lang, out HashSet<string>? wordList))
        {
            wordList = await LoadWordListAsync(lang, cancellationToken);
            _inMemoryCache[lang] = wordList;
        }

        return wordList.Contains(word);
    }
    
    private async Task<HashSet<string>> LoadWordListAsync(string lang, CancellationToken cancellationToken)
    {
        IEnumerable<string> words = await dbContext.Words
            .Where(w => w.Language == lang)
            .Select(w => w.Text)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
    }
    
    public async Task<Dictionary<WordKey, Word>> GetAllAsync(CancellationToken cancellationToken = new())
    {
        Dictionary<WordKey, Word> existingWords =
            await dbContext.Words
                .AsNoTracking()
                .ToDictionaryAsync(w => new WordKey(w.Language, w.Text), cancellationToken)
                .ConfigureAwait(false);

        return existingWords;
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
        string solutionCountCacheKey = $"{SolutionCountCachePrefix}_{lang}_{length}";
        int solutionCount =
            await cache.GetAsync<int?>(solutionCountCacheKey, cancellationToken).ConfigureAwait(false) ??
            await GetSolutionCountAsync(solutionCountCacheKey, lang, length, cancellationToken)
                .ConfigureAwait(false);

        int randomIndex = _random.Next(0, solutionCount);

        return await dbContext.Words
            .AsNoTracking()
            .Where(w => w.Language == lang && w.Length == length)
            .Select(w => w.Text)
            .Skip(randomIndex)
            .Take(1)
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<int> GetSolutionCountAsync(string solutionCountCacheKey, string language, int length,
        CancellationToken cancellationToken = new())
    {
        int solutionCount = await dbContext.Words
            .CountAsync(w => w.Language == language && w.IsSolution && w.Length == length, cancellationToken)
            .ConfigureAwait(false);
        await cache.SetAsync(solutionCountCacheKey, solutionCount, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return solutionCount;
    }
}