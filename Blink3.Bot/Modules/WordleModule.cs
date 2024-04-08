using Blink3.Core.Entities;
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
            text = $"**Correct!** You got it in {wordle.TotalAttempts} tries";
            await wordleRepository.DeleteAsync(wordle);
        }

        using MemoryStream image = new MemoryStream();
        await wordleGameService.GenerateImageAsync(guess, image);
        FileAttachment attachment = new(image, $"{guess.Word}.png");
        await FollowupWithFileAsync(text: text, attachment: attachment, ephemeral: false);
    }
}