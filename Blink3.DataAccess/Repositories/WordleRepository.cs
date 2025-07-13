using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class WordleRepository(BlinkDbContext dbContext) :
    BaseGameRepository<Wordle>(dbContext), IWordleRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public override async Task<Wordle?> GetByIdAsync(params object[] keyValues)
    {
        if (keyValues[0] is not int id) return null;

        return await _dbContext.Wordles
            .Include(w => w.Guesses)
            .FirstOrDefaultAsync(w => w.Id == id).ConfigureAwait(false);
    }

    public override async Task<Wordle?> GetByChannelIdAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wordles
            .Include(w => w.Guesses)
            .FirstOrDefaultAsync(w => w.ChannelId == channelId, cancellationToken).ConfigureAwait(false);
    }

    public Task AddGuessAsync(Wordle wordle, WordleGuess guess, CancellationToken cancellationToken = default)
    {
        _dbContext.Attach(wordle);
        wordle.Guesses.Add(guess);
        _dbContext.Entry(wordle).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}