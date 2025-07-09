using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Helpers;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class GameStatisticsModule(
    IUnitOfWork unitOfWork,
    ILogger<GameStatisticsModule> logger) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    [SlashCommand("statistics", "View your game statistics and compare them with your friends.")]
    public async Task Statistics(IUser? user = null)
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();
            IUser targetUser = user ?? Context.User;

            GameStatistics stats = await _unitOfWork.GameStatisticsRepository.GetGlobalStatisticsAsync(targetUser.Id);
 
            await RespondOrFollowUpAsync(components: GenerateGameStatisticsResponse(stats, targetUser, null),
                allowedMentions: new AllowedMentions(AllowedMentionTypes.Users));
            
            logger.LogInformation("{User} Viewed Global statistics for {TargetUser}",
                userLogContext, new UserLogContext(targetUser));
        }
    }

    private static MessageComponent GenerateGameStatisticsResponse(GameStatistics stats, IUser targetUser, GameType? gameType)
    {
        // Calculate the win percentage
        double winPercentage = stats.GamesPlayed > 0
            ? Math.Round((double)stats.GamesWon / stats.GamesPlayed * 100, 2)
            : 0;

        string game = gameType.GetFriendlyName();
        
        TimestampTag? lastActivity = null;
        TimestampTag? streakReset = null;
        TimestampTag? streakExpires = null;
        if (stats.LastActivity.HasValue)
        {
            lastActivity = TimestampTag.FromDateTime(stats.LastActivity.Value, TimestampTagStyles.Relative);

            DateTime nextDayStart = StreakHelpers.GetNextStreakDate(stats);
            streakReset = TimestampTag.FromDateTime(nextDayStart, TimestampTagStyles.Relative);

            DateTime streakExpiresDate = StreakHelpers.GetStreakExpiry(stats);
            streakExpires = TimestampTag.FromDateTime(streakExpiresDate, TimestampTagStyles.Relative);
        }

        ComponentBuilderV2 builder = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithAccentColor(Colours.Info)
                    .WithSection(new SectionBuilder()
                        .WithAccessory(new ThumbnailBuilder().WithMedia(new UnfurledMediaItemProperties
                        {
                            Url = targetUser.GetDisplayAvatarUrl(size: 240)
                        }))
                        .WithTextDisplay($"""
                                          ## {game} Statistics
                                          Here are the {game} statistics for {targetUser.Mention} {stats.CurrentStreak.GetStreakText()}
                                          """))
                    .WithSeparator(SeparatorSpacingSize.Large)
                    .WithTextDisplay("""
                                     ### 🎮 General Stats
                                     """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay($"""
                                      - **Games Played**: {stats.GamesPlayed}
                                      - **Games Won**: {stats.GamesWon}
                                      - **Win Percentage**: {winPercentage}%
                                      - **Points**: {stats.Points}
                                      """)
                    .WithSeparator(SeparatorSpacingSize.Large)
                    .WithTextDisplay("""
                                     ### 🔥 Streak Stats
                                     """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay($"""
                                      - **Current Streak**: {stats.CurrentStreak}
                                      - **Max Streak**: {stats.MaxStreak}
                                      - **Last Streak Update**: {lastActivity?.ToString() ?? "N/A"}
                                      - **Next Streak Day**: {streakReset?.ToString() ?? "N/A"}
                                      - **Streak Expires**: {streakExpires?.ToString() ?? "N/A"}
                                      """));

        return builder.Build();
    }
    
    [SlashCommand("leaderboard", "Check the top players and see how you rank.")]
    public async Task Leaderboard()
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();

            IEnumerable<GameStatistics> leaderboard = await _unitOfWork.GameStatisticsRepository.GetLeaderboardAsync();

            ContainerBuilder? containerBuilder = new ContainerBuilder()
                .WithAccentColor(Colours.Info)
                .WithTextDisplay("""
                                 ## Blink Leaderboard
                                 These are the top players right now — how do your stats compare?
                                 """);

            int i = 1;
            foreach (GameStatistics stats in leaderboard.Take(3))
            {
                IUser? discordUser = await Context.Client.GetUserAsync(stats.BlinkUserId);

                containerBuilder.WithSeparator(SeparatorSpacingSize.Large)
                    .WithTextDisplay($"### {i}. <@{stats.BlinkUserId}> {stats.CurrentStreak.GetStreakText()}")
                    .WithSection(new SectionBuilder()
                        .WithAccessory(new ThumbnailBuilder().WithMedia(new UnfurledMediaItemProperties
                        {
                            Url = discordUser.GetDisplayAvatarUrl(size: 240)
                        }))
                        .WithTextDisplay($"""
                                          - **Points**: {stats.Points}
                                          - **Games Played**: {stats.GamesPlayed}
                                          - **Games Won**: {stats.GamesWon}
                                          """)
                    );
                i++;
            }

            containerBuilder.WithSeparator();

            foreach (GameStatistics stats in leaderboard.Skip(3))
            {
                containerBuilder.WithTextDisplay($"""
                                                  ### {i}. <@{stats.BlinkUserId}> {stats.CurrentStreak.GetStreakText()}
                                                  - **Points** {stats.Points}
                                                  """);
                i++;
            }

            ComponentBuilderV2 builder = new ComponentBuilderV2().WithContainer(containerBuilder);

            await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
            logger.LogInformation("{User} Viewed the Global leaderboard", userLogContext);
        }
    }
}