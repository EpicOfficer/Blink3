using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Blink3.DataAccess.Extensions;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class WordleModule(
    IWordleRepository wordleRepository,
    IWordleGameService wordleGameService,
    IWordsClientService wordsClientService,
    IBlinkUserRepository blinkUserRepository,
    IWordRepository wordRepository) : BlinkModuleBase<IInteractionContext>
{
    [SlashCommand("wordle", "Start a new game of wordle")]
    public async Task Start()
    {
        await DeferAsync();

        if (await wordleGameService.IsGameInProgressAsync(Context.Channel.Id))
        {
            await RespondInfoAsync("Game already in progress",
                "A wordle is already in progress for this channel.  Type `/guess` to try and guess it");
            return;
        }

        _ = await wordleGameService.StartNewGameAsync(Context.Channel.Id, "en", 5);

        await RespondSuccessAsync("Wordle started", "A new wordle has started.  Type `/guess` guess it.", false);
    }

    [SlashCommand("guess", "Try to guess the wordle")]
    public async Task Guess(string word)
    {
        await DeferAsync();

        Wordle? wordle = await wordleRepository.GetByChannelIdAsync(Context.Channel.Id);
        if (wordle is null)
        {
            await RespondErrorAsync("No game in progress",
                "There is no game in progress.  Type `/wordle` to start one");
            return;
        }

        bool isGuessable = await wordRepository.IsGuessableAsync(word);
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

        WordleGuess guess = guessResult.SafeValue();
        string text = string.Empty;
        if (guess.IsCorrect)
        {
            text = $"**Correct!** You got it in {wordle.TotalAttempts} tries.  ";
            int pointsToAdd = 11 - wordle.TotalAttempts;
            if (pointsToAdd > 0)
            {
                BlinkUser user = await blinkUserRepository.GetOrCreateByIdAsync(Context.User.Id);
                user.Points += pointsToAdd;
                await blinkUserRepository.UpdateAsync(user);
                text += $"You have been awarded {pointsToAdd} points";
            }
            
            await wordleRepository.DeleteAsync(wordle);
        }

        using MemoryStream image = new();
        await wordleGameService.GenerateImageAsync(guess, image);
        using FileAttachment attachment = new(image, $"{guess.Word}.png");

        ComponentBuilder? component = new ComponentBuilder().WithButton("Define", $"blink-define-word_{guess.Word}");

        await FollowupWithFileAsync(text: text, attachment: attachment, ephemeral: false,
            components: component.Build());
    }

    [SlashCommand("define", "Get the definition of a word")]
    [ComponentInteraction("blink-define-word_*")]
    public async Task Define(string word)
    {
        WordDetails? details = null;
        try
        {
            details = await wordsClientService.GetDefinitionAsync(word);
        }
        catch
        {
            await RespondErrorAsync(word.ToTitleCase(), "An error occured fetching word definition",
                true);
            return;
        }

        EmbedFieldBuilder[]? groupedDefinitions = details?.Definitions
            .GroupBy(wd => wd.PartOfSpeech)
            .Select(g =>
            {
                EmbedFieldBuilder? builder = new()
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
            groupedDefinitions?.Length < 1 ? "Could not find a definition" : string.Empty,
            embedFields: groupedDefinitions,
            ephemeral: false);
    }
}