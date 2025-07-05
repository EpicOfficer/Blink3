using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Discord;

namespace Blink3.Scout.Jobs;

public abstract class BaseStreakJob(IServiceScopeFactory scopeFactory, ILogger logger)
{
    protected readonly IServiceScopeFactory ScopeFactory = scopeFactory;
    protected readonly ILogger Logger = logger;
    
    protected async Task<IUser?> FetchUserDetailsAsync(IDiscordClient client, ulong userId)
    {
        try
        {
            IUser? user = await client.GetUserAsync(userId);
            if (user != null)
                return user;

            Logger.LogWarning("User with ID {UserId} could not be found.", userId);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while fetching user details for userId {UserId}.", userId);
            return null;
        }
    }

    protected static async Task<IReadOnlyCollection<GameStatistics>> GetGameStatisticsAsync(IServiceScope scope)
    {
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return await unitOfWork.GameStatisticsRepository.GetAllAsync();
    }
}