using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Helpers;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[UsedImplicitly]
public class BlinkMixModule(
    IUnitOfWork unitOfWork,
    IBlinkMixGameService blinkMixGameService,
    ILogger<WordleModule> logger) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private ulong GameId => Context.Interaction.ChannelId ?? Context.User.Id;

    [SlashCommand("mix", "Start a game of BlinkMix, first to unscramble the word wins!")]
    public async Task Mix()
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();
            
            if (await blinkMixGameService.IsGameInProgressAsync(GameId))
            {
                logger.LogInformation("{User} Attempted to start a BlinkMix, but an active game was found with ID {GameId}",
                    userLogContext, GameId);
                await RespondInfoAsync("Game already in progress",
                    "A BlinkMix game is already in progress for this channel.  Type `/unmix` to try and guess it");
                return;
            }
            
            BlinkMix newGame = await blinkMixGameService.StartNewGameAsync(GameId);
            GameStatistics stats = await StreakHelpers.EnsureStatsUpdatedAsync(_unitOfWork, Context.User.Id, GameType.BlinkMix);
            await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
            await _unitOfWork.SaveChangesAsync();
            
            ComponentBuilderV2 builder = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithAccentColor(Colours.Success)
                    .WithTextDisplay("""
                                     ## A New BlinkMix Game Has Started! 🎉
                                     First to unscramble the word wins!
                                     """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay($"""
                                      ### 🌐 Game Details
                                      **{newGame.GetShuffledSolution()}**
                                      """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay("***Good luck, and have fun!***"));

            logger.LogInformation("{User} Started a new BlinkMix", userLogContext);
            await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
        }
    }

    [SlashCommand("unmix", "Guess the hidden answer in an active BlinkMix game")]
    public async Task Unmix(string guess)
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();
            BlinkMix? blinkMix = await _unitOfWork.BlinkMixRepository.GetByChannelIdAsync(GameId);
            if (blinkMix is null)
            {
                logger.LogInformation("{User} Tried to make a guess on an inactive BlinkMix game", userLogContext);
                await RespondErrorAsync("No game in progress",
                    "There is no active BlinkMix game in progress.  Type `/mix` to start one");
                return;
            }
            
            bool isCorrect = blinkMix.IsCorrectSolution(guess);

            // Update game statistics
            GameStatistics stats = await StreakHelpers.EnsureStatsUpdatedAsync(_unitOfWork, Context.User.Id, GameType.BlinkMix);
            blinkMix.AddPlayer(Context.User.Id);
            
            if (isCorrect)
            {
                // Update stats for a win
                stats.GamesWon++;
                stats.GamesPlayed++;
                stats.Points += blinkMix.GetScore();

                // Update statistics for all other participants
                HashSet<GameStatistics> participantStats =
                    await _unitOfWork.BlinkMixRepository.GetOtherParticipantStatsAsync(blinkMix, Context.User.Id);

                foreach (GameStatistics participant in participantStats)
                {
                    participant.GamesPlayed++;
                    await _unitOfWork.GameStatisticsRepository.UpdateAsync(participant);
                }
                
                // End the game
                _unitOfWork.BlinkMixRepository.Delete(blinkMix);
            }

            await RespondWithMessageAsync(blinkMix, stats, isCorrect);

            // Save updated statistics
            await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
            await _unitOfWork.SaveChangesAsync();

            logger.LogInformation("{User} Made a guess on a BlinkMix game ({Guess}) - Correct: {IsCorrect}",
                userLogContext, guess, isCorrect);
        }
    }

    private async Task RespondWithMessageAsync(BlinkMix game, GameStatistics stats, bool isCorrect)
    {
        int pointsAwarded = game.GetScore();

        string responseMessage = isCorrect
            ? $"""
               🎉 **Congratulations, <@{stats.BlinkUserId}>!** You solved the BlinkMix!
               You've earned **{pointsAwarded} points**, bringing your total to **{stats.Points} points**.
               """
            : """
              ⚠️ **Not quite!** Keep trying, you’ve got this!
              """;

        ContainerBuilder container = new ContainerBuilder()
            .WithAccentColor(isCorrect ? Colours.Success : Colours.Danger)
            .WithTextDisplay(responseMessage);

        // If correct, show the solution explicitly
        if (isCorrect)
        {
            container.WithSeparator(isDivider: false)
                .WithTextDisplay($"### The solution was: **{game.Solution}**");
        }

        ComponentBuilderV2 builder = new ComponentBuilderV2()
            .WithContainer(container);

        await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
    }
}