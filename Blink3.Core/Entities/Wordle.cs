// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a Wordle game.
/// </summary>
public class Wordle
{
    /// <summary>
    ///     Gets or sets the ID of the Wordle Game.
    /// </summary>
    /// <remarks>
    ///     If used in a guild, this will be the channel ID
    ///     In case it is used in DM or through the API, will be the user ID
    /// </remarks>
    [Key]
    [Required]
    public ulong Id { get; set; }

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