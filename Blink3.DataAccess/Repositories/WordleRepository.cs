using System.Threading.Tasks;
using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blink3.DataAccess.Repositories;

public class WordleRepository(BlinkDbContext dbContext) :
    GenericRepository<Wordle>(dbContext), IWordleRepository
{
    private readonly BlinkDbContext _dbContext = dbContext;

    public override async Task<Wordle?> GetByIdAsync(params object[] keyValues)
    {
        if (keyValues[0] is not int id) return default;

        return await _dbContext.Wordles
            .Include(w => w.Guesses)
            .FirstOrDefaultAsync(w => w.Id == id).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _dbContext.Wordles.AnyAsync(w => w.Id == id).ConfigureAwait(false);
    }
}