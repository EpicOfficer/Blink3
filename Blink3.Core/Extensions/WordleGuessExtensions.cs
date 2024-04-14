using Blink3.Core.Entities;
using Blink3.Core.Enums;

namespace Blink3.Core.Extensions;

/// <summary>
///     Provides extension methods for the WordleGuess class.
/// </summary>
public static class WordleGuessExtensions
{
    /// <summary>
    ///     Marks the correct letters in a WordleGuess object based on the provided Wordle object and a collection of correct
    ///     indices.
    /// </summary>
    /// <param name="guess">The WordleGuess object to mark.</param>
    /// <param name="wordle">The Wordle object containing the word to guess.</param>
    /// <param name="correctIndices">The list to store the indices of correct letters.</param>
    public static void MarkCorrectLetters(this WordleGuess guess, Wordle wordle, ICollection<int> correctIndices)
    {
        string word = guess.Word;
        string wordToGuess = wordle.WordToGuess;
        for (int i = 0; i < wordToGuess.Length; i++)
        {
            if (wordToGuess[i] != word[i]) continue;
            guess.Letters[i].State = WordleLetterStateEnum.Correct;
            correctIndices.Add(i);
        }
    }

    /// <summary>
    ///     Marks the misplaced letters in the WordleGuess object based on the comparison with the Wordle object.
    /// </summary>
    /// <param name="guess">The WordleGuess object to mark the misplaced letters for.</param>
    /// <param name="wordle">The Wordle object to compare the guess against.</param>
    /// <param name="correctIndices">The collection of indices that represent the correct letters.</param>
    /// <param name="misplacedIndices">The list to store the indices of the misplaced letters.</param>
    public static void MarkMisplacedLetters(this WordleGuess guess, Wordle wordle,
        ICollection<int> correctIndices, ICollection<int> misplacedIndices)
    {
        string word = guess.Word;
        string wordToGuess = wordle.WordToGuess;
        Dictionary<char, int> checkedLettersCount = new Dictionary<char, int>();  // number of times a letter has been processed

        for (int i = 0; i < word.Length; i++)
        {
            char letter = word[i];
            if (guess.Letters[i].State == WordleLetterStateEnum.Correct) continue;

            checkedLettersCount.TryAdd(letter, 0);
            int letterOccurrences = wordToGuess.AllIndexesOf(letter).Count;

            if (checkedLettersCount[letter] >= letterOccurrences) continue;
            checkedLettersCount[letter]++;

            List<int> indices = wordToGuess.AllIndexesOf(letter);
            foreach (int index in indices.Where(index =>
                         !(correctIndices.Contains(index) || misplacedIndices.Contains(index))))
            {
                guess.Letters[i].State = WordleLetterStateEnum.Misplaced;
                misplacedIndices.Add(index);
                break;
            }
        }
    }
}