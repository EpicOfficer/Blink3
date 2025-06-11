using Blink3.Bot.Enums;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class WordleModule(
    IWordleRepository wordleRepository,
    IWordleGameService wordleGameService,
    IWordsClientService wordsClientService,
    IBlinkGuildRepository blinkGuildRepository,
    IGameStatisticsRepository gameStatisticsRepository,
    IWordRepository wordRepository) : BlinkModuleBase<IInteractionContext>(blinkGuildRepository)
{
    private ulong GameId => Context.Interaction.ChannelId ?? Context.User.Id;

    private async Task<GameStatistics> UpdateStatsAsync(ulong userId)
    {
        GameStatistics stats = await gameStatisticsRepository.GetOrCreateGameStatistics(userId, GameType.Wordle);
        DateTime time = DateTime.UtcNow;

        // If the last activity is on the same date, no need to update the streak
        if (stats.LastActivity?.Date ==
            time.Date) return stats; // Do nothing except return the stats (they already participated today)

        // If activity is on the next consecutive UTC day, increment the streak
        if (stats.LastActivity?.Date.AddDays(1) == time.Date)
        {
            stats.CurrentStreak++; // Increment the streak
            stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak); // Update max streak if needed
            stats.LastActivity = time; // Update the last activity to today
            await gameStatisticsRepository.UpdateAsync(stats);
            return stats;
        }

        // If it's been more than 1 day, reset the streak
        stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak); // Update max streak before resetting
        stats.CurrentStreak = 0; // Reset current streak
        stats.LastActivity = time; // Update the last activity
        await gameStatisticsRepository.UpdateAsync(stats);

        return stats;
    }

    [SlashCommand("wordle", "Start a new game of wordle")]
    public async Task Start(WordleLanguageEnum language = WordleLanguageEnum.English)
    {
        await DeferAsync();

        if (await wordleGameService.IsGameInProgressAsync(GameId))
        {
            await RespondInfoAsync("Game already in progress",
                "A wordle is already in progress for this channel.  Type `/guess` to try and guess it");
            return;
        }

        string lang = language switch
        {
            WordleLanguageEnum.Spanish => "es",
            _ => "en"
        };

        string langString = language switch
        {
            WordleLanguageEnum.Spanish => "Spanish",
            _ => "English"
        };

        await wordleGameService.StartNewGameAsync(GameId, lang, 5);
        GameStatistics stats = await UpdateStatsAsync(Context.User.Id);
        await gameStatisticsRepository.UpdateAsync(stats);

        List<EmbedFieldBuilder> fields =
        [
            new()
            {
                Name = "Language",
                Value = langString
            }
        ];

        await RespondSuccessAsync("Wordle started", "A new wordle has started.  Type `/guess` guess it.", false,
            embedFields: fields.ToArray());
    }

    [SlashCommand("guess", "Try to guess the wordle")]
    public async Task Guess(string word)
    {
        await DeferAsync();

        Wordle? wordle = await wordleRepository.GetByChannelIdAsync(GameId);
        if (wordle is null)
        {
            await RespondErrorAsync("No game in progress",
                "There is no game in progress.  Type `/wordle` to start one");
            return;
        }

        bool isGuessable = await wordRepository.IsGuessableAsync(word, wordle.Language);
        if (!isGuessable)
        {
            await RespondErrorAsync("Invalid guess", "The word you entered is not a valid guess.");
            return;
        }

        Result<WordleGuess> guessResult = await wordleGameService.MakeGuessAsync(word, Context.User.Id, wordle);
        if (!guessResult.IsSuccess)
        {
            await RespondErrorAsync("Invalid guess", guessResult.Error ?? "An unspecified error occured.");
            return;
        }

        GameStatistics stats = await UpdateStatsAsync(Context.User.Id);

        if (wordle.Players.Contains(Context.User.Id) is false) wordle.Players.Add(Context.User.Id);

        WordleGuess guess = guessResult.SafeValue();
        string text = string.Empty;
        if (guess.IsCorrect)
        {
            stats.GamesWon++;
            stats.GamesPlayed++;

            int pointsToAdd = 11 - wordle.TotalAttempts;
            stats.Points += Math.Max(pointsToAdd, 0);

            text =
                $"🎉 **Correct!** You solved it in **{wordle.TotalAttempts} attempt{(wordle.TotalAttempts > 1 ? "s" : "")}**.\n" +
                $"You earned **{pointsToAdd} point{(pointsToAdd != 1 ? "s" : "")}**, and now have a total of **{stats.Points} point{(stats.Points != 1 ? "s" : "")}**. 🎯";

            foreach (ulong player in wordle.Players.ToHashSet().Where(u => u != Context.User.Id))
            {
                GameStatistics playerStats =
                    await gameStatisticsRepository.GetOrCreateGameStatistics(player, GameType.Wordle);
                playerStats.GamesPlayed++;
                await gameStatisticsRepository.UpdateAsync(playerStats);
            }

            await wordleRepository.DeleteAsync(wordle);
        }
        else
        {
            await wordleRepository.UpdateAsync(wordle);
        }

        await gameStatisticsRepository.UpdateAsync(stats);

        using MemoryStream image = new();
        await wordleGameService.GenerateImageAsync(guess, image, await FetchConfig());
        using FileAttachment attachment = new(image, $"{wordle.Id}_{guess.Id}.png");

        ComponentBuilder? component = null;
        if (wordle.Language == "en")
            component = new ComponentBuilder().WithButton("Define", $"blink-define-word_{guess.Word}");

        await FollowupWithFileAsync(text: text, attachment: attachment, ephemeral: false,
            components: component?.Build());
    }

    [SlashCommand("define", "Get the definition of a word")]
    [ComponentInteraction("blink-define-word_*")]
    public async Task Define(string word)
    {
        await DeferAsync();
        WordDetails? details;
        try
        {
            details = await wordsClientService.GetDefinitionAsync(word);
        }
        catch
        {
            await RespondErrorAsync(word.ToTitleCase(), "An error occured fetching word definition");
            return;
        }

        EmbedFieldBuilder[]? groupedDefinitions = details?.Definitions
            .GroupBy(wd => wd.PartOfSpeech)
            .Select(g =>
            {
                EmbedFieldBuilder builder = new()
                {
                    Name = g.Key.ToTitleCase(),
                    Value = string.Join("\n",
                            g.Select(v => $"- {v.Definition.ToSentenceCase()}"))
                        .TruncateTo(1020),
                    IsInline = false
                };
                return builder;
            })
            .ToArray();

        await RespondPlainAsync(
            $"Definition of {word.ToTitleCase()}",
            details is null ? "No definition found" : string.Empty,
            embedFields: groupedDefinitions,
            ephemeral: false);
    }

    [SlashCommand("statistics", "View your Wordle game statistics")]
    public async Task Statistics()
    {
        await DeferAsync();

        // Retrieve the user's game statistics
        GameStatistics stats =
            await gameStatisticsRepository.GetOrCreateGameStatistics(Context.User.Id, GameType.Wordle);

        // Calculate the win percentage
        double winPercentage = stats.GamesPlayed > 0
            ? Math.Round((double)stats.GamesWon / stats.GamesPlayed * 100, 2)
            : 0;

        TimestampTag? lastActivity = null;
        TimestampTag? streakReset = null;
        TimestampTag? streakExpires = null;
        if (stats.LastActivity.HasValue)
        {
            lastActivity = TimestampTag.FromDateTime(stats.LastActivity.Value, TimestampTagStyles.Relative);

            DateTime nextDayStart = stats.LastActivity.Value.Date.AddDays(1);
            streakReset = TimestampTag.FromDateTime(nextDayStart, TimestampTagStyles.Relative);

            DateTime streakExpiresDate = stats.LastActivity.Value.Date.AddDays(2);
            streakExpires = TimestampTag.FromDateTime(streakExpiresDate, TimestampTagStyles.Relative);
        }

        ComponentBuilderV2 builder = new ComponentBuilderV2()
            .WithContainer(new ContainerBuilder()
                .WithAccentColor(Colours.Info)
                .WithSection(new SectionBuilder()
                    .WithAccessory(new ThumbnailBuilder().WithMedia(new UnfurledMediaItemProperties
                    {
                        Url = Context.User.GetDisplayAvatarUrl(size: 240)
                    }))
                    .WithTextDisplay($"""
                                      ## Wordle Statistics
                                      Here are your Wordle statistics, {Context.User.Mention}
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

        await RespondOrFollowUpAsync(components: builder.Build());
    }
    
    [SlashCommand("leaderboard", "Display points leaderboard")]
    public async Task Leaderboard()
    {
        await DeferAsync();
        
        IEnumerable<GameStatistics> leaderboard = await gameStatisticsRepository.GetLeaderboardAsync(GameType.Wordle);
        
        ContainerBuilder? containerBuilder = new ContainerBuilder()
            .WithAccentColor(Colours.Info)
            .WithTextDisplay("""
                             ## Wordle Leaderboard
                             These are the top Blink Wordle players — how do your stats compare?
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
        
        await RespondOrFollowUpAsync(components: builder.Build());
    }
}