using Blink3.Core.Entities;
using Blink3.Core.Repositories.Interfaces;
using Discord;
using Discord.Interactions;

namespace Blink3.Bot.Modules;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class WordleModule(IWordleRepository wordleRepository,
    IWordleGuessRepository wordleGuessRepository,
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
        
        bool isGuessable = await wordRepository.IsGuessableAsync(word);
        if (!isGuessable)
        {
            await RespondErrorAsync("Invalid guess", "The word you entered is not valid.");
            return;
        }

        await RespondInfoAsync("Valid guess", "That's a valid guess", ephemeral: false);
    }
}