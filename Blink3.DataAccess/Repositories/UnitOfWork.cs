using Blink3.Core.Caching;
using Blink3.Core.Interfaces;
using Blink3.Core.Repositories.Interfaces;

namespace Blink3.DataAccess.Repositories;

public class UnitOfWork(BlinkDbContext context, ICachingService cache) : IUnitOfWork
{
    // Private fields for repositories
    private IBlinkGuildRepository? _blinkGuildRepository;
    private IBlinkUserRepository? _blinkUserRepository;
    private IGameStatisticsRepository? _gameStatisticsRepository;
    private ITempVcRepository? _tempVcRepository;
    private IUserTodoRepository? _userTodoRepository;
    private IWordleRepository? _wordleRepository;
    private IWordRepository? _wordRepository;

    public IBlinkGuildRepository BlinkGuildRepository =>
        _blinkGuildRepository ??= new BlinkGuildRepository(context, cache);

    public IBlinkUserRepository BlinkUserRepository =>
        _blinkUserRepository ??= new BlinkUserRepository(context);

    public ITempVcRepository TempVcRepository =>
        _tempVcRepository ??= new TempVcRepository(context);

    public IWordleRepository WordleRepository =>
        _wordleRepository ??= new WordleRepository(context);

    public IWordRepository WordRepository =>
        _wordRepository ??= new WordRepository(context, cache);

    public IUserTodoRepository UserTodoRepository =>
        _userTodoRepository ??= new UserTodoRepository(context);

    public IGameStatisticsRepository GameStatisticsRepository =>
        _gameStatisticsRepository ??= new GameStatisticsRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}