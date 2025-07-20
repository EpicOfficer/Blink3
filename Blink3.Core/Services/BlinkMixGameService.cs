using Blink3.Core.Entities;
using Blink3.Core.Interfaces;

namespace Blink3.Core.Services;

public class BlinkMixGameService(IUnitOfWork unitOfWork) : IBlinkMixGameService
{
    public async Task<bool> IsGameInProgressAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.BlinkMixRepository.GetByChannelIdAsync(channelId, cancellationToken)
            .ConfigureAwait(false) is not null;
    }

    public async Task<BlinkMix> StartNewGameAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        int length = Random.Shared.Next(5, 8);
        string word = await unitOfWork.WordRepository.GetRandomSolutionAsync("en", length, cancellationToken)
            .ConfigureAwait(false);

        BlinkMix newMix = new()
        {
            ChannelId = channelId,
            Solution = word
        };
        
        await unitOfWork.BlinkMixRepository.AddAsync(newMix, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return newMix;
    }
}