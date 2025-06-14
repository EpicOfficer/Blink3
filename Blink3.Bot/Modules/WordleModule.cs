using Blink3.Bot.Enums;
using Blink3.Bot.Extensions;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class WordleModule(
    IUnitOfWork unitOfWork,
    IWordleGameService wordleGameService,
    IWordsClientService wordsClientService) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private ulong GameId => Context.Interaction.ChannelId ?? Context.User.Id;

    private async Task<GameStatistics> UpdateStatsAsync(ulong userId)
    {
        GameStatistics stats =
            await _unitOfWork.GameStatisticsRepository.GetOrCreateGameStatistics(userId, GameType.Wordle);
        DateTime time = DateTime.UtcNow;

        // If the last activity is on the same date, no need to update the streak
        if (stats.LastActivity?.Date ==
            time.Date) return stats; // Do nothing except return the stats (they already participated today)

        bool isConsecutiveDay = stats.LastActivity?.Date.AddDays(1) == time.Date;
        if (isConsecutiveDay)
        {
            stats.CurrentStreak++; // Increment the streak
            stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak); // Update max streak if needed
        }
        else
        {
            stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak);
            stats.CurrentStreak = 0;
        }

        stats.LastActivity = time; // Update the last activity
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
        await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
        await _unitOfWork.SaveChangesAsync();

        ComponentBuilderV2 builder = new ComponentBuilderV2()
            .WithContainer(new ContainerBuilder()
                .WithAccentColor(Colours.Success)
                .WithTextDisplay("""
                                 ## Wordle Started
                                 A new Wordle game has started! Use `/guess` to make your guesses.
                                 """)
                .WithSeparator(isDivider: false)
                .WithTextDisplay($"""
                                  ### 🌐 Game Details
                                  - **Language:** {langString}
                                  - **Word Length:** 5 letters
                                  """)
                .WithSeparator(isDivider: false)
                .WithTextDisplay("***Good luck, and have fun!***"));

        await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
    }

    [SlashCommand("guess", "Try to guess the wordle")]
    public async Task Guess(string word)
    {
        await DeferAsync();

        Wordle? wordle = await _unitOfWork.WordleRepository.GetByChannelIdAsync(GameId);
        if (wordle is null)
        {
            await RespondErrorAsync("No game in progress",
                "There is no game in progress.  Type `/wordle` to start one");
            return;
        }

        bool isGuessable = await _unitOfWork.WordRepository.IsGuessableAsync(word, wordle.Language);
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
        Color embedColor = Colours.Info;

        if (guess.IsCorrect)
        {
            stats.GamesWon++;
            stats.GamesPlayed++;

            int pointsToAdd = Math.Max(11 - wordle.TotalAttempts, 0);
            stats.Points += pointsToAdd;

            text = $"""
                    🎉 **Congratulations, <@{stats.BlinkUserId}>!** You solved the Wordle in **{wordle.TotalAttempts} attempt{(wordle.TotalAttempts == 1 ? "" : "s")}**!  
                    You've earned **{pointsToAdd} point{(pointsToAdd == 1 ? "" : "s")}**, and now have a total of **{stats.Points}** point{(stats.Points == 1 ? "" : "s")}.
                    """;
            embedColor = Colours.Success;

            HashSet<GameStatistics> playerStatsList =
                await _unitOfWork.WordleRepository.GetOtherParticipantStatsAsync(wordle, Context.User.Id);

            // Update stats for other players who participated
            foreach (GameStatistics playerStats in playerStatsList)
            {
                playerStats.GamesPlayed++;
                await _unitOfWork.GameStatisticsRepository.UpdateAsync(playerStats);
            }

            await _unitOfWork.WordleRepository.DeleteAsync(wordle);
        }
        else
        {
            await _unitOfWork.WordleRepository.UpdateAsync(wordle);

            text = $"""
                    ⚠️ **Not quite!** Keep trying, you’ve got this!  
                    You've used **{wordle.TotalAttempts} attempt{(wordle.TotalAttempts == 1 ? "" : "s")}** so far.
                    """;
        }

        await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
        await _unitOfWork.SaveChangesAsync();

        BlinkGuild config = await FetchConfig();
        using MemoryStream image = new();
        await wordleGameService.GenerateImageAsync(guess, image, config);
        using FileAttachment attachment = new(image, $"{wordle.Id}_{guess.Id}.png");

        ContainerBuilder container = new ContainerBuilder()
            .WithAccentColor(embedColor)
            .WithTextDisplay(text)
            .WithMediaGallery([attachment.GetAttachmentUrl()]);

        ButtonBuilder defineButton = new("Define", $"blink-define-word_{guess.Word}");
        ButtonBuilder letterButton = new("Show letters", $"blink-wordle-status_{wordle.Id}", ButtonStyle.Secondary);
        switch (wordle.Language)
        {
            case "en" when !guess.IsCorrect:
                container.WithActionRow(new ActionRowBuilder()
                    .WithButton(defineButton)
                    .WithButton(letterButton));
                break;
            case "es" when !guess.IsCorrect:
                container.WithActionRow(new ActionRowBuilder()
                    .WithButton(letterButton));
                break;
        }
        
        ComponentBuilderV2 builder = new ComponentBuilderV2().WithContainer(container);
        await FollowupWithFileAsync(attachment,
            components: builder.Build(),
            ephemeral: false);
    }

    [ComponentInteraction("blink-wordle-status_*")]
    public async Task Status(int id)
    {
        await DeferAsync();
        Wordle? wordle = await _unitOfWork.WordleRepository.GetByIdAsync(id);
        if (wordle is null)
        {
            await RespondErrorAsync("Invalid wordle", "This is not the button you are looking for.");
            return;
        }
        
        using MemoryStream image = new();
        await wordleGameService.GenerateStatusImageAsync(wordle, image);
        using FileAttachment attachment = new(image, $"{wordle.Id}_status.png");

        ComponentBuilderV2 builder = new ComponentBuilderV2()
            .WithContainer(new ContainerBuilder()
                .WithAccentColor(Colours.Info)
                .WithTextDisplay("Here are the letters that have been eliminated so far...")
                .WithMediaGallery([attachment.GetAttachmentUrl()])
            );
        
        await FollowupWithFileAsync(attachment,
            components: builder.Build(),
            ephemeral: true);
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

        ContainerBuilder container = new ContainerBuilder()
            .WithAccentColor(Colours.Info)
            .WithTextDisplay($"## Definition of {word.ToTitleCase()}");

        IMessageComponentBuilder[]? definitions = details?.Definitions
            .GroupBy(wd => wd.PartOfSpeech)
            .SelectMany(g => new IMessageComponentBuilder[]
            {
                new SeparatorBuilder(false),
                new TextDisplayBuilder($"""
                                        ### {g.Key.ToTitleCase()}
                                        {string.Join("\n", g.Select(v => $"- {v.Definition.ToSentenceCase()}"))}
                                        """)
            })
            .ToArray();

        if (definitions?.Length > 0)
            container.AddComponents(definitions);
        else
            container.WithTextDisplay("No definitions found");

        ComponentBuilderV2 builder = new ComponentBuilderV2().WithContainer(container);
        await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None,
            ephemeral: true);
    }

    [SlashCommand("statistics", "View game statistics")]
    public async Task Statistics(IUser? user = null)
    {
        await DeferAsync();
        IUser targetUser = user ?? Context.User;

        // Retrieve the user's game statistics
        GameStatistics stats =
            await _unitOfWork.GameStatisticsRepository.GetOrCreateGameStatistics(targetUser.Id, GameType.Wordle);

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
                        Url = targetUser.GetDisplayAvatarUrl(size: 240)
                    }))
                    .WithTextDisplay($"""
                                      ## Wordle Statistics
                                      Here are the Wordle statistics for {targetUser.Mention} {stats.CurrentStreak.GetStreakText()}
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

        await RespondOrFollowUpAsync(components: builder.Build(),
            allowedMentions: new AllowedMentions(AllowedMentionTypes.Users));
    }

    [SlashCommand("leaderboard", "Display points leaderboard")]
    public async Task Leaderboard()
    {
        await DeferAsync();

        IEnumerable<GameStatistics> leaderboard =
            await _unitOfWork.GameStatisticsRepository.GetLeaderboardAsync(GameType.Wordle);

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

        await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
    }
}