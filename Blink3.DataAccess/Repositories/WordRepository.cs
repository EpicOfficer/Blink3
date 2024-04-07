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

    public async Task<bool> IsGuessableAsync(string word, string lang = "en",
        CancellationToken cancellationToken = new())
    {
#pragma warning disable CA1862
        return await dbContext.Words.AnyAsync(w => w.Text.ToLower() == word.ToLower() &&
#pragma warning restore CA1862
                                                   w.Language == lang, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetRandomSolutionAsync(int length = 5, string lang = "en",
        CancellationToken cancellationToken = new())
    {
        string solutionCountCacheKey = $"{SolutionCountCachePrefix}_{lang}_{length}";
        int solutionCount = await cache.GetAsync<int?>(solutionCountCacheKey).ConfigureAwait(false) ??
                            await GetSolutionCountAsync(solutionCountCacheKey, lang, length, cancellationToken)
                                .ConfigureAwait(false);

        int randomIndex = _random.Next(0, solutionCount);

        return await dbContext.Words.Where(w => w.Language == lang && w.Length == length)
            .Select(w => w.Text)
            .Skip(randomIndex)
            .Take(1)
            .SingleAsync()
            .ConfigureAwait(false);
    }

    public async Task<Dictionary<WordKey, Word>> GetAllAsync(CancellationToken cancellationToken = new())
    {
        Dictionary<WordKey, Word> existingWords =
            await dbContext.Words.ToDictionaryAsync(w => new WordKey(w.Language, w.Text), cancellationToken)
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

    private async Task<int> GetSolutionCountAsync(string solutionCountCacheKey, string language, int length,
        CancellationToken cancellationToken = new())
    {
        int solutionCount = await dbContext.Words
            .CountAsync(w => w.Language == language && w.IsSolution && w.Length == length, cancellationToken)
            .ConfigureAwait(false);
        await cache.SetAsync(solutionCountCacheKey, solutionCount).ConfigureAwait(false);
        return solutionCount;
    }
}