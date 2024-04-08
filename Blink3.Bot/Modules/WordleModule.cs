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
public class WordleModule(IWordleRepository wordleRepository,
    IWordleGameService wordleGameService,
    IWordRepository wordRepository) : BlinkModuleBase<IInteractionContext>
{
    [SlashCommand("wordle", "Start a new game of wordle")]
    public async Task Start()
    {
        await DeferAsync(false);

        if (await wordleRepository.GetByChannelIdAsync(Context.Channel.Id) is not null)
        {
            await RespondInfoAsync("Game already in progress",
                "A wordle is already in progress for this channel.  Type `/guess` to try and guess it");
            return;
        }
        
        string word = await wordRepository.GetRandomSolutionAsync();
        Wordle wordle = await wordleRepository.AddAsync(new Wordle
        {
            ChannelId = Context.Channel.Id,
            Language = "en",
            WordToGuess = word
        });
        
        await RespondSuccessAsync("Wordle started", "A new wordle has started.  Type `/guess` guess it.", ephemeral: false);
    }

    [SlashCommand("guess", "Try to guess the wordle")]
    public async Task Guess(string word)
    {
        await DeferAsync(false);
        
        Wordle? wordle = await wordleRepository.GetByChannelIdAsync(Context.Channel.Id);
        if (wordle is null)
        {
            await RespondErrorAsync("No game in progress", "There is no game in progress.  Type `/wordle` to start one");
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
        
        MemoryStream img = await wordleGameService.GenerateImageAsync(guess);
        FileAttachment attachment = new(img, $"{guess.Word}.png");
        await FollowupWithFileAsync(text: text, attachment: attachment, ephemeral: false);
    }
}