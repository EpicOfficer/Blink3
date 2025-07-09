using Blink3.Bot.Enums;
using Blink3.Bot.MessageStyles;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Helpers;
using Blink3.Core.Interfaces;
using Blink3.Core.LogContexts;
using Blink3.Core.Models;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class WordleModule(
    IUnitOfWork unitOfWork,
    IWordleGameService wordleGameService,
    IWordsClientService wordsClientService,
    ILogger<WordleModule> logger) : BlinkModuleBase<IInteractionContext>(unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private static readonly ObjectPool<MemoryStream> StreamPool =
        new DefaultObjectPool<MemoryStream>(new DefaultPooledObjectPolicy<MemoryStream>(), 20);
    
    private ulong GameId => Context.Interaction.ChannelId ?? Context.User.Id;

    [SlashCommand("word", "Start a game of BlinkWord, a fun word-guessing challenge!")]
    public async Task Start(WordleLanguageEnum language = WordleLanguageEnum.English)
    {
        const int wordLength = 5;
        
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();

            if (await wordleGameService.IsGameInProgressAsync(GameId))
            {
                logger.LogInformation("{User} Attempted to start a BlinkWord, but an active game was found with ID {GameId}",
                    userLogContext, GameId);
                await RespondInfoAsync("Game already in progress",
                    "A BlinkWord game is already in progress for this channel.  Type `/guess` to try and guess it");
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

            await wordleGameService.StartNewGameAsync(GameId, lang, wordLength);
            GameStatistics stats = await StreakHelpers.EnsureStatsUpdatedAsync(_unitOfWork, Context.User.Id, GameType.BlinkWord);
            await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
            await _unitOfWork.SaveChangesAsync();

            ComponentBuilderV2 builder = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithAccentColor(Colours.Success)
                    .WithTextDisplay("""
                                     ## A New BlinkWord Game Has Started! 🎉
                                     Your mission, should you choose to accept it, is to guess the **hidden word** as quickly as possible.
                                     """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay($"""
                                      ### 🌐 Game Details
                                      - **Language:** {langString}
                                      - **Word Length:** {wordLength} letters
                                      """)
                    .WithSeparator(isDivider: false)
                    .WithTextDisplay("***Good luck, and have fun!***"));

            logger.LogInformation("{User} Started a new {Language}, {WordLength} letter BlinkWord ",
                userLogContext, langString, wordLength);;
            await RespondOrFollowUpAsync(components: builder.Build(), allowedMentions: AllowedMentions.None);
        }
    }

    [SlashCommand("guess", "Submit a word to guess the hidden answer in an active BlinkWord game")]
    public async Task Guess(string word)
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            Wordle? wordle = await _unitOfWork.WordleRepository.GetByChannelIdAsync(GameId);
            if (wordle is null)
            {
                logger.LogInformation("{User} Tried to make a guess on an inactive BlinkWord game", userLogContext);
                await RespondErrorAsync("No game in progress",
                    "There is no active BlinkWord game in progress.  Type `/word` to start one");
                return;
            }

            bool isGuessable = await _unitOfWork.WordRepository.IsGuessableAsync(word, wordle.Language);
            if (!isGuessable)
            {
                logger.LogInformation("{User} Tried to guess an invalid word", userLogContext);
                await RespondErrorAsync("Invalid guess", "The word you entered is not a valid guess.");
                return;
            }

            // Make a Guess
            Result<WordleGuess> guessResult = await wordleGameService.MakeGuessAsync(word, Context.User.Id, wordle);
            if (!guessResult.IsSuccess)
            {
                logger.LogInformation("{User} Triggered an error while making a guess: {Message}", userLogContext, guessResult.Error);
                await RespondErrorAsync("Invalid guess", guessResult.Error ?? "An unspecified error occured.");
                return;
            }

            // Update game statistics
            GameStatistics stats = await StreakHelpers.EnsureStatsUpdatedAsync(_unitOfWork, Context.User.Id, GameType.BlinkWord);
            if (wordle.Players.Contains(Context.User.Id) is false)
                wordle.Players.Add(Context.User.Id);

            WordleGuess guess = guessResult.SafeValue();
            string responseMessage = BuildGuessResponse(wordle, guess, stats);
            await HandleGameUpdates(wordle, stats, guess, responseMessage);
            logger.LogInformation("{User} Made a guess on a BlinkWord game", userLogContext);
        }
    }

    private static string BuildGuessResponse(Wordle wordle, WordleGuess guess, GameStatistics stats)
    {
        if (guess.IsCorrect)
        {
            stats.GamesWon++;
            stats.GamesPlayed++;
            int pointsToAdd = Math.Max(11 - wordle.TotalAttempts, 0);
            stats.Points += pointsToAdd;

            return $"""
                    🎉 **Congratulations, <@{stats.BlinkUserId}>!** You solved the BlinkWord in **{wordle.TotalAttempts} attempt{(wordle.TotalAttempts == 1 ? "" : "s")}**!  
                    You've earned **{pointsToAdd} point{(pointsToAdd == 1 ? "" : "s")}**, and now have a total of **{stats.Points}** point{(stats.Points == 1 ? "" : "s")}.
                    """;
        }

        return $"""
                ⚠️ **Not quite!** Keep trying, you’ve got this!  
                You've used **{wordle.TotalAttempts} attempt{(wordle.TotalAttempts == 1 ? "" : "s")}** so far.
                """;
    }
    
    private async Task HandleGameUpdates(Wordle wordle, GameStatistics stats, WordleGuess guess, string responseMessage)
    {
        Color embedColor = guess.IsCorrect ? Colours.Success : Colours.Info;

        if (guess.IsCorrect)
        {
            HashSet<GameStatistics> playerStatsList =
                await _unitOfWork.WordleRepository.GetOtherParticipantStatsAsync(wordle, Context.User.Id);

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
        }

        await _unitOfWork.GameStatisticsRepository.UpdateAsync(stats);
        await _unitOfWork.SaveChangesAsync();

        // Generate game response
        await GenerateGuessResponse(wordle, guess, responseMessage, embedColor);
    }
    
    private async Task GenerateGuessResponse(Wordle wordle, WordleGuess guess, string text, Color embedColor)
    {
        ButtonBuilder defineButton = new("Define", $"blink-define-word_{guess.Word}");
        ButtonBuilder letterButton = new("Show letters", $"blink-wordle-status_{wordle.Id}", ButtonStyle.Secondary);

        MemoryStream image = StreamPool.Get();
        try
        {
            image.SetLength(0);
            BlinkGuild config = await FetchConfig();
            await wordleGameService.GenerateImageAsync(guess, image, config);
            image.Position = 0;

            FileAttachment attachment = new(image, $"{wordle.Id}_{guess.Id}.png");

            ContainerBuilder container = new ContainerBuilder()
                .WithAccentColor(embedColor)
                .WithTextDisplay(text)
                .WithMediaGallery([attachment.GetAttachmentUrl()]);

            switch (wordle.Language)
            {
                case "en" when !guess.IsCorrect:
                    container.WithActionRow(new ActionRowBuilder()
                        .WithButton(defineButton)
                        .WithButton(letterButton));
                    break;
                case "en" when guess.IsCorrect:
                    container.WithActionRow(new ActionRowBuilder()
                        .WithButton(defineButton));
                    break;
                case "es" when !guess.IsCorrect:
                    container.WithActionRow(new ActionRowBuilder()
                        .WithButton(letterButton));
                    break;
            }

            ComponentBuilderV2 builder = new ComponentBuilderV2().WithContainer(container);
            await RespondWithFileAsync(attachment, components: builder.Build(), ephemeral: false);
        }
        finally
        {
            StreamPool.Return(image);
        }
    }
    
    [ComponentInteraction("blink-wordle-status_*")]
    public async Task Status(int id)
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();
            Wordle? wordle = await _unitOfWork.WordleRepository.GetByIdAsync(id);
            if (wordle is null)
            {
                logger.LogInformation("{User} Tried to view the status of an invalid BlinkWord", userLogContext);
                await RespondErrorAsync("Invalid BlinkWord", "This is not the button you are looking for.");
                return;
            }
        
            MemoryStream image = StreamPool.Get();
            try
            {
                image.SetLength(0);
                await wordleGameService.GenerateStatusImageAsync(wordle, image);
            
                image.Position = 0;
                FileAttachment attachment = new(image, $"{wordle.Id}_status.png");

                ComponentBuilderV2 builder = new ComponentBuilderV2()
                    .WithContainer(new ContainerBuilder()
                        .WithAccentColor(Colours.Info)
                        .WithTextDisplay("Here are the letters that have been eliminated so far...")
                        .WithMediaGallery([attachment.GetAttachmentUrl()])
                    );
        
                await FollowupWithFileAsync(attachment,
                    components: builder.Build(),
                    ephemeral: true);
                logger.LogInformation("{User} Viewed the status of a BlinkWord", userLogContext);
            }
            finally
            {
                StreamPool.Return(image);
            }
        }
    }

    [SlashCommand("define", "Get the definition of a word")]
    [ComponentInteraction("blink-define-word_*")]
    public async Task Define(string word)
    {
        UserLogContext userLogContext = new(Context.User);
        using (logger.BeginScope(new { User = userLogContext }))
        {
            await DeferAsync();
            WordDetails? details;
            try
            {
                details = await wordsClientService.GetDefinitionAsync(word);
            }
            catch (Exception e)
            {
                logger.LogError(e, "{User} Failed to get the definition of a word", userLogContext);
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
            logger.LogInformation("{User} Viewed the definition of a word", userLogContext);
        }
    }
}