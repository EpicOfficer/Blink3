using Blink3.Core.Entities;

namespace Blink3.Core.Interfaces;

public interface IBlinkMixGameService
{
    public Task<bool> IsGameInProgressAsync(ulong channelId, CancellationToken cancellationToken = default);
    public Task<BlinkMix> StartNewGameAsync(ulong channelId, CancellationToken cancellationToken = default);
}