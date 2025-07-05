using Blink3.Core.Entities;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Blink3.Core.Helpers;
using Discord;
using Discord.Rest;

namespace Blink3.Scout.Jobs;

// ReSharper disable once ClassNeverInstantiated.Global
public class StreakResetJob(IServiceScopeFactory scopeFactory, ILogger<StreakResetJob> logger) : BaseStreakJob(scopeFactory, logger)
{
    public async Task ExecuteAsync()
    {
        using IServiceScope scope = ScopeFactory.CreateScope();
        
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        DiscordRestClient client = scope.ServiceProvider.GetRequiredService<DiscordRestClient>();
        IReadOnlyCollection<GameStatistics> gameStats = await GetGameStatisticsAsync(scope);
        DateTime now = DateTime.UtcNow;

        logger.LogInformation("Processing streak resets for {TotalUsers} users at {CurrentTime}", gameStats.Count, now);
        
        foreach (GameStatistics gameStat in gameStats)
        {
            if (StreakHelpers.ShouldResetStreak(gameStat))
            {
                await HandleStreakResetAsync(client, gameStat, unitOfWork, now);
            }
        }
        
        await unitOfWork.SaveChangesAsync();
    }
    
    private async Task HandleStreakResetAsync(IDiscordClient client, GameStatistics gameStat, IUnitOfWork unitOfWork, DateTime now)
    {
        IUser? user = await FetchUserDetailsAsync(client, gameStat.BlinkUserId);
        if (user != null)
        {
            Logger.LogInformation("Resetting streak for {User}...", new UserLogContext(user));
        }
        else
        {
            Logger.LogInformation("Resetting streak for unknown user (ID: {UserId}).", gameStat.BlinkUserId);
        }

        StreakHelpers.ResetStreak(gameStat);
        await unitOfWork.GameStatisticsRepository.UpdateAsync(gameStat);
    }
}