using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Tests.Helpers;

namespace Blink3.Tests.Core.Extensions;

[TestFixture]
public class WordleExtensionsTests
{
    [Test]
    public void ProcessGuess_MixtureOfCorrectMisplacedAndIncorrectLetters_ShouldMarkAccordingly()
    {
        // Arrange
        WordleGuess guess = WordleHelpers.CreateGuess("alien");
        Wordle wordle = new() { WordToGuess = "apple" };
        
        // Act
        wordle.ProcessGuess(guess);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(guess.Letters[0].State, Is.EqualTo(WordleLetterStateEnum.Correct));
            Assert.That(guess.Letters[1].State, Is.EqualTo(WordleLetterStateEnum.Misplaced));
            Assert.That(guess.Letters[2].State, Is.EqualTo(WordleLetterStateEnum.Incorrect));
            Assert.That(guess.Letters[3].State, Is.EqualTo(WordleLetterStateEnum.Misplaced));
            Assert.That(guess.Letters[4].State, Is.EqualTo(WordleLetterStateEnum.Incorrect));
        });
    }
    
    [Test]
    public void ValidateWordLength_WordAndGuessSameLength_ShouldReturnTrue()
    {
        // Arrange
        Wordle wordle = new() { WordToGuess = "apple" };
        string guess = "grunt";
        
        // Act
        bool isSameLength = wordle.ValidateWordLength(guess);
        
        // Assert
        Assert.That(isSameLength, Is.True);
    }
    
    [Test]
    public void ValidateWordLength_WordAndGuessDifferentLength_ShouldReturnFalse()
    {
        // Arrange
        Wordle wordle = new() { WordToGuess = "apple" };
        string guess = "grunts";
        
        // Act
        bool isSameLength = wordle.ValidateWordLength(guess);
        
        // Assert
        Assert.That(isSameLength, Is.False);
    }
}