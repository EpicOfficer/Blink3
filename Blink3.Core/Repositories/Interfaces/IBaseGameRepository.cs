using Blink3.Core.Base;
using Blink3.Core.Entities;

namespace Blink3.Core.Repositories.Interfaces;

public interface IBaseGameRepository<TGame> where TGame : GameBase
{
    public Task<TGame?> GetByIdAsync(params object[] keyValues);

    public Task<TGame?> GetByChannelIdAsync(ulong channelId, CancellationToken cancellationToken = default);

    public Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<HashSet<GameStatistics>> GetOtherParticipantStatsAsync(TGame game, ulong userId,
        CancellationToken cancellationToken = default);

    public Task<TGame> AddAsync(TGame entity, CancellationToken cancellationToken = default);

    public TGame Update(TGame entity);

    public void Delete(TGame entity);
}