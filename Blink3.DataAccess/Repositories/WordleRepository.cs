using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class WordleRepository(BlinkDbContext dbContext) :
    GenericRepository<Wordle>(dbContext), IWordleRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public override async Task<Wordle?> GetByIdAsync(params object[] keyValues)
    {
        if (keyValues[0] is not int id) return null;

        return await _dbContext.Wordles
            .Include(w => w.Guesses)
            .FirstOrDefaultAsync(w => w.Id == id).ConfigureAwait(false);
    }

    public async Task<Wordle?> GetByChannelIdAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wordles
            .Include(w => w.Guesses)
            .FirstOrDefaultAsync(w => w.ChannelId == channelId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wordles.AnyAsync(w => w.Id == id, cancellationToken).ConfigureAwait(false);
    }

    public Task AddGuessAsync(Wordle wordle, WordleGuess guess, CancellationToken cancellationToken = default)
    {
        _dbContext.Attach(wordle);
        wordle.Guesses.Add(guess);
        _dbContext.Entry(wordle).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<HashSet<GameStatistics>> GetOtherParticipantStatsAsync(Wordle wordle, ulong userId,
        CancellationToken cancellationToken = default)
    {
        HashSet<ulong> players = new(wordle.Players);
        List<GameStatistics> stats = await _dbContext.GameStatistics
            .AsNoTracking()
            .Where(s => players.Contains(s.BlinkUserId) &&
                        s.Type == GameType.Wordle &&
                        s.BlinkUserId != userId)
            .ToListAsync(cancellationToken);
        return [..stats];
    }
}