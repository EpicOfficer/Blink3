using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Interfaces;
using Blink3.Core.Repositories.Interfaces;

namespace Blink3.Core.Services;

/// <summary>
///     Represents a service for playing the Wordle game.
/// </summary>
public class WordleGameService(IWordleGuessRepository wordleGuessRepository) : IWordleGameService
{
    public async Task<WordleGuess> MakeGuessAsync(string word, ulong userId, Wordle wordle)
    {
        if (wordle.WordToGuess.Length != word.Length)
            throw new InvalidOperationException("Provided word is the wrong length for this wordle");

        WordleGuess? oldGuess = wordleGuessRepository.GetByWord(wordle, word);
        if (oldGuess is not null) return oldGuess;
        
        WordleGuess guess = new()
        {
            WordleId = wordle.Id,
            GuessedById = userId
        };

        string wordToGuess = wordle.WordToGuess;
        for (int i = 0; i < word.Length; i++)
        {
            WordleLetterStateEnum state = WordleLetterStateEnum.Incorrect;

            if (word[i] == wordToGuess[i])
                state = WordleLetterStateEnum.Correct;
            else if (wordToGuess.Contains(word[i])) state = WordleLetterStateEnum.Misplaced;

            guess.Letters.Add(new WordleLetter
            {
                Position = i,
                Letter = word[i],
                State = state
            });
        }

        return await wordleGuessRepository.AddAsync(guess);
    }
}