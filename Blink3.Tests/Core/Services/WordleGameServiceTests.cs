using Blink3.Core.Entities;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Blink3.Core.Repositories.Interfaces;
using Blink3.Core.Services;
using Moq;

namespace Blink3.Tests.Core.Services;

[TestFixture]
public class WordleGameServiceTests
{
    [SetUp]
    public void Setup()
    {
        _wordleGameService = new WordleGameService(
            _wordRepositoryMock.Object,
            _wordleRepositoryMock.Object,
            _guessImageGeneratorMock.Object);
    }

    private readonly Mock<IWordRepository> _wordRepositoryMock = new();
    private readonly Mock<IWordleRepository> _wordleRepositoryMock = new();
    private readonly Mock<IWordleGuessImageGenerator> _guessImageGeneratorMock = new();
    private WordleGameService? _wordleGameService;
    private const ulong ChannelId = 123;

    [Test]
    public async Task Test_IsGameInProgressAsync()
    {
        // Arrange
        _wordleRepositoryMock
            .Setup(x => x.GetByChannelIdAsync(ChannelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Wordle());

        // Act
        Assert.That(_wordleGameService, Is.Not.Null);
        bool result = await _wordleGameService!.IsGameInProgressAsync(ChannelId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Test_IsGameInProgressAsync_NoGameInProgress()
    {
        // Arrange
        _wordleRepositoryMock
            .Setup(x => x.GetByChannelIdAsync(ChannelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wordle?)null);

        // Act
        Assert.That(_wordleGameService, Is.Not.Null);
        bool result = await _wordleGameService!.IsGameInProgressAsync(ChannelId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Test_StartNewGameAsync()
    {
        const string language = "en";
        const int length = 5;
        const string word = "guess";

        // Setup mock to return word when GetRandomSolutionAsync is called
        _wordRepositoryMock
            .Setup(x => x.GetRandomSolutionAsync(language, length, It.IsAny<CancellationToken>()))
            .ReturnsAsync(word);

        // Setup mock to not do anything when AddAsync is called 
        // (since we're testing StartNewGameAsync, not AddAsync)
        _wordleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Wordle>(), It.IsAny<CancellationToken>()))
            .Returns<Wordle, CancellationToken>((wordle, _) => Task.FromResult(wordle));

        // Call the method under test
        Assert.That(_wordleGameService, Is.Not.Null);
        Wordle result = await _wordleGameService!.StartNewGameAsync(ChannelId, language, length);

        Assert.Multiple(() =>
        {
            // Verify that the returned Wordle has the expected properties
            Assert.That(result.ChannelId, Is.EqualTo(ChannelId));
            Assert.That(result.Language, Is.EqualTo(language));
            Assert.That(result.WordToGuess, Is.EqualTo(word));
        });

        // Verify that AddAsync was called with the correct Wordle
        _wordleRepositoryMock.Verify(x =>
            x.AddAsync(It.Is<Wordle>(w =>
                w.ChannelId == ChannelId &&
                w.Language == language &&
                w.WordToGuess == word), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Test_MakeCorrectGuessAsync()
    {
        const string word = "apple";
        const ulong userId = 123;
        Wordle wordle = new() { WordToGuess = word };

        // Setup mock to not do anything when AddGuessAsync is called
        _wordleRepositoryMock.Setup(x =>
                x.AddGuessAsync(It.IsAny<Wordle>(), It.IsAny<WordleGuess>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Call the method under test
        Result<WordleGuess> result = await _wordleGameService!.MakeGuessAsync(word, userId, wordle);

        Assert.Multiple(() =>
        {
            // Verify that the result is successful
            Assert.That(result.IsSuccess, Is.True);

            // Verify that the returned WordleGuess has the expected properties
            Assert.That(result.SafeValue().Word, Is.EqualTo(word));
            Assert.That(result.SafeValue().IsCorrect, Is.True);
            Assert.That(result.SafeValue().Letters.Any(w => w.State != WordleLetterStateEnum.Correct), Is.False);
        });

        // Verify that AddGuessAsync was called with the correct Wordle and WordleGuess
        _wordleRepositoryMock.Verify(x =>
            x.AddGuessAsync(It.Is<Wordle>(w => w.WordToGuess == word),
                It.Is<WordleGuess>(g => g.Word == word),
                It.IsAny<CancellationToken>()));
    }
}