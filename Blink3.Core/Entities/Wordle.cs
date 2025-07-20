// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;
using Blink3.Core.Base;
using Blink3.Core.Enums;

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a Wordle game.
/// </summary>
public class Wordle : GameBase
{
    public override GameType Type => GameType.BlinkWord;
    
    /// <summary>
    ///     The language this wordle was started in
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    ///     Represents the word to guess in the Wordle game.
    /// </summary>
    [Required]
    [MaxLength(8)]
    public string WordToGuess { get; set; } = string.Empty;

    /// <summary>
    ///     The total number of guesses in this wordle game.
    /// </summary>
    public int TotalAttempts => Guesses.Count;

    /// <summary>
    ///     Represents the collection of guesses made by players in a Wordle game.
    /// </summary>
    public ICollection<WordleGuess> Guesses { get; set; } = [];
}