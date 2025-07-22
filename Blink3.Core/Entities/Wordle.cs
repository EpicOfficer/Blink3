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
    
    private readonly string _language = string.Empty;
    private readonly string _wordToGuess = string.Empty;

    
    /// <summary>
    ///     The language this Wordle was started in.
    /// </summary>
    public string Language
    {
        get => _language;
        init => _language = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Represents the word to guess in the Wordle game.
    /// </summary>
    [Required]
    [MaxLength(8)]
    public string WordToGuess
    {
        get => _wordToGuess;
        init => _wordToGuess = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Represents the collection of guesses made by players in a Wordle game.
    /// </summary>
    public ICollection<WordleGuess> Guesses { get; set; } = [];
    
    /// <summary>
    ///     The total number of guesses in this wordle game.
    /// </summary>
    public int TotalAttempts => Guesses.Count;
}