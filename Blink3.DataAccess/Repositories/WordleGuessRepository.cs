using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;

namespace Blink3.DataAccess.Repositories;

public class WordleGuessRepository(BlinkDbContext dbContext) :
    GenericRepository<WordleGuess>(dbContext), IWordleGuessRepository
{
}