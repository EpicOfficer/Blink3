namespace Blink3.Core.Entities;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

/// <summary>
///     Represents a Wordle game.
/// </summary>
public class Wordle
{
    /// <summary>
    ///     Gets or sets the ID of the Wordle Game.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Represents the word to guess in the Wordle game.
    /// </summary>
    public string WordToGuess { get; set; } = string.Empty;

    /// <summary>
    ///     The total number of guesses in this wordle game.
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    ///     Gets or sets the current guess in a Wordle game.
    /// </summary>
    /// <remarks>
    ///     This property represents the current guess made by the player in a Wordle game.
    ///     The current guess is a WordleWord object, which contains the letters guessed by the player.
    /// </remarks>
    public WordleWord CurrentGuess { get; set; } = new();
}