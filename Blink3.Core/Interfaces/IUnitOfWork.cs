using Blink3.Core.Repositories.Interfaces;

namespace Blink3.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Encapsulated access to repositories
    IBlinkGuildRepository BlinkGuildRepository { get; }
    IBlinkUserRepository BlinkUserRepository { get; }
    ITempVcRepository TempVcRepository { get; }
    IWordleRepository WordleRepository { get; }
    IBlinkMixRepository BlinkMixRepository { get; }
    IWordRepository WordRepository { get; }
    IUserTodoRepository UserTodoRepository { get; }
    IGameStatisticsRepository GameStatisticsRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // Saves all changes
}