using System.Text;
using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;
using Blink3.Core.Repositories.Interfaces;
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
        await DeferAsync();

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
        
        await RespondSuccessAsync($"Wordle {wordle.Id} started", $"Test word is {wordle.WordToGuess} in lang {wordle.Language}", ephemeral: false);
    }

    [SlashCommand("guess", "Try to guess the wordle")]
    public async Task Guess(string word)
    {
        await DeferAsync();
        
        Wordle? wordle = await wordleRepository.GetByChannelIdAsync(Context.Channel.Id);
        if (wordle is null)
        {
            await RespondErrorAsync("No game in progress", "There is no game in progress.  Type `/wordle` to start one");
            return;
        }

        if (word.Length != wordle.WordToGuess.Length)
        {
            await RespondErrorAsync("Invalid guess", "The word you entered is not a valid guess.");
            return;
        }
        
        bool isGuessable = await wordRepository.IsGuessableAsync(word);
        if (!isGuessable)
        {
            await RespondErrorAsync("Invalid guess", "The word you entered is not a valid guess.");
            return;
        }

        WordleGuess guess = await wordleGameService.MakeGuessAsync(word, Context.User.Id, wordle);
        if (guess.IsCorrect)
        {
            await RespondSuccessAsync("Correct");
        }
        else
        {
            StringBuilder sb = new();

            foreach (WordleLetter letter in guess.Letters)
            {
                switch (letter.State)
                {
                    case WordleLetterStateEnum.Incorrect:
                        sb.Append($":x: {letter.Letter}");
                        break;
                    case WordleLetterStateEnum.Misplaced:
                        sb.Append($":warning: {letter.Letter}");
                        break;
                    case WordleLetterStateEnum.Correct:
                        sb.Append($":white_check_mark: {letter.Letter}");
                        break;
                    default:
                        break;
                }
            }
            
            await RespondInfoAsync("Nice try", sb.ToString(), ephemeral: false);
        }
    }
}