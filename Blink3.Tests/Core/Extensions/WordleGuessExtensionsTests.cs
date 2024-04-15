using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Tests.Helpers;

namespace Blink3.Tests.Core.Extensions;

[TestFixture]
public class WordleGuessExtensionsTests
{
    [Test]
    public void MarkCorrectLetters_AllLettersCorrect_ShouldMarkAllLettersCorrect()
    {
        // Arrange
        Wordle wordle = new() { WordToGuess = "apple" };
        WordleGuess guess = WordleHelpers.CreateGuess("apple");
        List<int> correctIndices = [];
        
        // Act
        guess.MarkCorrectLetters(wordle, correctIndices);
        
        // Assert 
        foreach (WordleLetter letter in guess.Letters)
        {
            Assert.That(letter.State, Is.EqualTo(WordleLetterStateEnum.Correct));
        }

        for (int i = 0; i < wordle.WordToGuess.Length; i++)
        {
            Assert.That(correctIndices, Does.Contain(i));
        }
        
        Assert.That(guess.IsCorrect, Is.EqualTo(true));
    }
    
    [Test]
    public void MarkCorrectLetters_NoLettersCorrect_ShouldNotMarkAnyLetterCorrect()
    {
        // Arrange
        Wordle wordle = new() { WordToGuess = "grunt" };
        WordleGuess guess = WordleHelpers.CreateGuess("apple");
        List<int> correctIndices = [];

        // Act
        guess.MarkCorrectLetters(wordle, correctIndices);

        // Assert
        foreach (WordleLetter letter in guess.Letters)
        {
            Assert.That(letter.State, Is.Not.EqualTo(WordleLetterStateEnum.Correct));
        }

        Assert.Multiple(() =>
        {
            Assert.That(correctIndices, Is.Empty);
            Assert.That(guess.IsCorrect, Is.EqualTo(false));
        });
    }

    [Test]
    public void MarkMisplacedLetters_AllLettersMisplaced_ShouldMarkAllLettersMisplaced()
    {
        // Arrange
        WordleGuess guess = WordleHelpers.CreateGuess("paepl");
        Wordle wordle = new() { WordToGuess = "apple" };
        List<int> correctIndices = [];
        List<int> misplacedIndices = [];
        
        // Act
        guess.MarkMisplacedLetters(wordle, correctIndices, misplacedIndices);
        
        // Assert
        foreach (WordleLetter letter in guess.Letters)
        {
            Assert.That(letter.State, Is.EqualTo(WordleLetterStateEnum.Misplaced));
        }
        
        for (int i = 0; i < wordle.WordToGuess.Length; i++)
        {
            Assert.That(misplacedIndices, Does.Contain(i));
        }

        Assert.Multiple(() =>
        {
            Assert.That(correctIndices, Is.Empty);
            Assert.That(guess.IsCorrect, Is.EqualTo(false));
        });
    }

    [Test]
    public void MarkMisplacedLetters_NoLettersMisplaced_ShouldNotMarkAnyLetterMisplaced()
    {
        // Arrange
        WordleGuess guess = WordleHelpers.CreateGuess("youth");
        Wordle wordle = new() { WordToGuess = "apple" };
        List<int> correctIndices = [];
        List<int> misplacedIndices = [];
        
        // Act
        guess.MarkMisplacedLetters(wordle, correctIndices, misplacedIndices);
        
        // Assert
        foreach (WordleLetter letter in guess.Letters)
        {
            Assert.That(letter.State, Is.EqualTo(WordleLetterStateEnum.Incorrect));
        }

        Assert.Multiple(() =>
        {
            Assert.That(correctIndices, Is.Empty);
            Assert.That(misplacedIndices, Is.Empty);
            Assert.That(guess.IsCorrect, Is.EqualTo(false));
        });
    }

    [Test]
    public void
        MarkMisplacedLetters_WordContainsTwoOfSameLetterAndGuessContainsTwoOfSameMisplaced_ShouldMarkThemMisplaced()
    {
        // Arrange
        WordleGuess guess = WordleHelpers.CreateGuess("onion");
        Wordle wordle = new Wordle { WordToGuess = "pools" };
        List<int> correctIndices = [];
        List<int> misplacedIndices = [];
        
        // Act
        guess.MarkMisplacedLetters(wordle, correctIndices, misplacedIndices);
        
        // Assert
        foreach (WordleLetter letter in guess.Letters)
        {
            Assert.That(letter.State,
                letter.Letter == 'o' // If letter is 'o'
                    ? Is.EqualTo(WordleLetterStateEnum.Misplaced)
                    : Is.Not.EqualTo(WordleLetterStateEnum.Misplaced));
        }

        Assert.Multiple(() =>
        {
            Assert.That(correctIndices, Has.Count.EqualTo(0));
            Assert.That(misplacedIndices, Has.Count.EqualTo(2));
            Assert.That(guess.Letters.Count(s => s.State == WordleLetterStateEnum.Misplaced), Is.EqualTo(2));
            Assert.That(guess.IsCorrect, Is.EqualTo(false));
        });
    }
}