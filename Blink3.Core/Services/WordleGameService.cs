using Blink3.Core.Entities;
using Blink3.Core.Extensions;
using Blink3.Core.Factories;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Blink3.Core.Services.Generators;

namespace Blink3.Core.Services;

/// <summary>
///     Represents a service for playing the Wordle game.
/// </summary>
public class WordleGameService(
    IUnitOfWork unitOfWork,
    IWordleGuessImageGenerator guessImageGenerator) : IWordleGameService
{
    public async Task<bool> IsGameInProgressAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.WordleRepository.GetByChannelIdAsync(channelId, cancellationToken)
                .ConfigureAwait(false) is not
            null;
    }

    public async Task<Result<WordleGuess>> MakeGuessAsync(string word, ulong userId, Wordle wordle,
        CancellationToken cancellationToken = default)
    {
        if (wordle.ValidateWordLength(word) is not true)
            return Result<WordleGuess>.Fail(
                $"The word you guessed does not match the wordle length {wordle.WordToGuess.Length}");

        WordleGuess? oldGuess = wordle.Guesses.FirstOrDefault(w => w.Word == word);
        if (oldGuess is not null) return Result<WordleGuess>.Ok(oldGuess);

        WordleGuess guess = WordleGuessFactory.Create(wordle, word, userId);

        wordle.ProcessGuess(guess);

        await unitOfWork.WordleRepository.AddGuessAsync(wordle, guess, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<WordleGuess>.Ok(guess);
    }

    public async Task GenerateStatusImageAsync(Wordle wordle,
        MemoryStream outStream,
        CancellationToken cancellationToken = default)
    {
        await guessImageGenerator.CreateAndSaveStatusImageAsync(wordle, outStream, cancellationToken);
    }

    public async Task GenerateImageAsync(WordleGuess guess, MemoryStream outStream, BlinkGuild blinkGuild,
        CancellationToken cancellationToken = default)
    {
        WordleGuessImageGeneratorOptions options = new()
        {
            BackgroundColour = blinkGuild.ParsedBackgroundColor,
            TextColour = blinkGuild.ParsedTextColour,
            CorrectTileColour = blinkGuild.ParsedCorrectTileColour,
            MisplacedTileColour = blinkGuild.ParsedMisplacedTileColour,
            IncorrectTileColour = blinkGuild.ParsedIncorrectTileColour
        };

        await guessImageGenerator.GenerateImageAsync(guess, options, outStream, cancellationToken);
    }

    public async Task<Wordle> StartNewGameAsync(ulong channelId, string language, int length,
        CancellationToken cancellationToken = default)
    {
        string word = await unitOfWork.WordRepository.GetRandomSolutionAsync(language, length, cancellationToken)
            .ConfigureAwait(false);
        Wordle newWordle = new()
        {
            ChannelId = channelId,
            Language = language,
            WordToGuess = word
        };
        await unitOfWork.WordleRepository.AddAsync(newWordle, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return newWordle;
    }
}