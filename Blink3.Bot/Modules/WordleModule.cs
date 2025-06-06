using Blink3.Bot.Enums;
using Blink3.Bot.Extensions;
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
    IBlinkUserRepository blinkUserRepository,
    IGameStatisticsRepository gameStatisticsRepository,
    IWordRepository wordRepository) : BlinkModuleBase<IInteractionContext>(blinkGuildRepository)
{
    private ulong GameId => Context.Interaction.ChannelId ?? Context.User.Id;

    private async Task<GameStatistics> UpdateStatsAsync(ulong userId)
    {
        GameStatistics stats = await gameStatisticsRepository.GetOrCreateGameStatistics(userId, GameType.Wordle);
        DateTime time = DateTime.UtcNow;
        
        if (time - stats.LastActivity < TimeSpan.FromDays(1))
        {
            stats.LastActivity = time;
            return stats;
        }

        if (time - stats.LastActivity < TimeSpan.FromDays(2))
        {
            stats.CurrentStreak++;
            stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak);
            stats.LastActivity = time;
            return stats;
        }
        
        stats.CurrentStreak = 0;
        stats.MaxStreak = Math.Max(stats.MaxStreak, stats.CurrentStreak);
        stats.LastActivity = time;
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

        if (wordle.Players.Contains(Context.User.Id) is false)
        {
            wordle.Players.Add(Context.User.Id);
        }
        
        WordleGuess guess = guessResult.SafeValue();
        string text = string.Empty;
        if (guess.IsCorrect)
        {
            stats.GamesWon++;
            stats.GamesPlayed++;
            text = $"**Correct!** You got it in {wordle.TotalAttempts} tries.  ";
            int pointsToAdd = 11 - wordle.TotalAttempts;
            if (pointsToAdd > 0)
            {
                BlinkUser user = await blinkUserRepository.GetOrCreateByIdAsync(Context.User.Id);
                user.Points += pointsToAdd;
                await blinkUserRepository.UpdateAsync(user);
                text += $"You have been awarded {pointsToAdd} points";
            }
            
            foreach (ulong player in wordle.Players.ToHashSet().Where(u => u != Context.User.Id))
            {
                GameStatistics playerStats = await gameStatisticsRepository.GetOrCreateGameStatistics(player, GameType.Wordle);
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

    [SlashCommand("points", "Show off your points!")]
    public async Task Points()
    {
        int points = (await blinkUserRepository.GetByIdAsync(Context.User.Id))?.Points ?? 0;
        await RespondPlainAsync($"You currently have {points} point{(points != 1 ? "s" : null)}.", ephemeral: false);
    }

    [SlashCommand("statistics", "View your Wordle game statistics")]
    public async Task Statistics()
    {
        await DeferAsync();

        // Retrieve the user's game statistics
        GameStatistics stats = await gameStatisticsRepository.GetOrCreateGameStatistics(Context.User.Id, GameType.Wordle);

        // Calculate the win percentage
        double winPercentage = stats.GamesPlayed > 0
            ? Math.Round((double)stats.GamesWon / stats.GamesPlayed * 100, 2)
            : 0;

        // Build an embed response
        EmbedFieldBuilder[] fields =
        [
            new()
            {
                Name = "Games Played",
                Value = $"{stats.GamesPlayed}"
            },
            new()
            {
                Name = "Games Won",
                Value = $"{stats.GamesWon}"
            },
            new()
            {
                Name = "Win Percentage",
                Value = $"{winPercentage}%"
            },
            new()
            {
                Name = "Current Streak",
                Value = $"{stats.CurrentStreak}"
            },
            new()
            {
                Name = "Max Streak",
                Value = $"{stats.MaxStreak}"
            }
        ];

        await RespondPlainAsync(
            "Your Wordle Statistics",
            embedFields: fields,
            ephemeral: false
        );
    }
    
    [SlashCommand("leaderboard", "Display points leaderboard")]
    public async Task Leaderboard()
    {
        IEnumerable<BlinkUser> leaderboard = await blinkUserRepository.GetLeaderboardAsync();

        IEnumerable<Task<EmbedFieldBuilder>> embedFieldBuilderTasks = leaderboard.Select(async (blinkUser, ix) =>
        {
            IUser? discordUser = await Context.Client.GetUserAsync(blinkUser.Id);
            string name = discordUser.GetFriendlyName();
            EmbedFieldBuilder field = new()
            {
                Name = $"{ix + 1}. {name}",
                Value = $"{blinkUser.Points} Point{(blinkUser.Points != 1 ? "s" : null)}",
                IsInline = false
            };
            return field;
        });

        EmbedFieldBuilder[] embedFieldBuilder = await Task.WhenAll(embedFieldBuilderTasks);

        await RespondPlainAsync("Points Leaderboard",
            embedFields: embedFieldBuilder,
            ephemeral: false);
    }
}