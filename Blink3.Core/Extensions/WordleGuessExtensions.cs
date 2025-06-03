using System.Globalization;
using System.Text;
using Blink3.Core.Entities;
using Blink3.Core.Enums;

// ReSharper disable SuggestBaseTypeForParameter

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
        string word = NormalizeWord(guess.Word);
        string wordToGuess = NormalizeWord(wordle.WordToGuess);

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
        string word = NormalizeWord(guess.Word);
        string wordToGuess = NormalizeWord(wordle.WordToGuess);

        Dictionary<char, int> checkedLettersCount = new();
        Dictionary<char, List<int>> charIndicesMap = GenerateCharIndicesMap(wordToGuess);

        for (int i = 0; i < word.Length; i++)
            MarkSpecificLetter(guess, word, correctIndices, misplacedIndices, checkedLettersCount, charIndicesMap, i);
    }

    /// <summary>
    ///     Generates a character indices map for a given word.
    /// </summary>
    /// <param name="wordToGuess">The word to generate the character indices map for.</param>
    /// <returns>A dictionary with characters as keys and lists of indices as values.</returns>
    private static Dictionary<char, List<int>> GenerateCharIndicesMap(string wordToGuess)
    {
        Dictionary<char, List<int>> charIndicesMap = new(wordToGuess.Length);

        for (int i = 0; i < wordToGuess.Length; i++)
        {
            char normalizedChar = NormalizeChar(wordToGuess[i]);
            if (!charIndicesMap.TryGetValue(normalizedChar, out List<int>? value))
            {
                value = [];
                charIndicesMap[normalizedChar] = value;
            }

            value.Add(i);
        }

        return charIndicesMap;
    }

    /// <summary>
    ///     Marks a specific letter in the WordleGuess object based on given parameters.
    /// </summary>
    /// <param name="guess">The WordleGuess object</param>
    /// <param name="word">The word from the guess</param>
    /// <param name="correctIndices">The collection of correct indices</param>
    /// <param name="misplacedIndices">The collection of misplaced indices</param>
    /// <param name="checkedLettersCount">The dictionary of checked letters count</param>
    /// <param name="charIndicesMap">The dictionary mapping characters to indices</param>
    /// <param name="i">The index of the letter to mark</param>
    private static void MarkSpecificLetter(WordleGuess guess, string word, ICollection<int> correctIndices,
        ICollection<int> misplacedIndices, Dictionary<char, int> checkedLettersCount,
        Dictionary<char, List<int>> charIndicesMap, int i)
    {
        char letter = NormalizeChar(word[i]);
        if (guess.Letters[i].State == WordleLetterStateEnum.Correct) return;

        checkedLettersCount.TryAdd(letter, 0);
        checkedLettersCount[letter]++;

        if (!charIndicesMap.TryGetValue(letter, out List<int>? value)) return;

        foreach (int index in value.Where(index =>
                     !correctIndices.Contains(index) && !misplacedIndices.Contains(index)))
        {
            guess.Letters[i].State = WordleLetterStateEnum.Misplaced;
            misplacedIndices.Add(index);
            break;
        }

        if (checkedLettersCount[letter] >= value.Count) charIndicesMap.Remove(letter);
    }

    /// <summary>
    ///     Normalizes a word by removing diacritical marks and converting it to lowercase.
    /// </summary>
    /// <param name="word">The word to normalize.</param>
    /// <returns>A normalized string without diacritical marks and in lowercase.</returns>
    private static string NormalizeWord(string word)
    {
        return string.Concat(word.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            .ToLowerInvariant();
    }

    /// <summary>
    ///     Normalizes a character by removing any diacritical marks and converting it to lowercase.
    /// </summary>
    /// <param name="letter">The character to normalize.</param>
    /// <returns>The normalized character.</returns>
    private static char NormalizeChar(char letter)
    {
        string normalizedString = NormalizeWord(letter.ToString());
        return normalizedString.Length > 0 ? normalizedString[0] : letter;
    }
}