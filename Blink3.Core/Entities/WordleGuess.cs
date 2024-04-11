using System.ComponentModel.DataAnnotations;
using Blink3.Core.Caching.Interfaces;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a word entered by a player in the Wordle game.
/// </summary>
public class WordleGuess : ICacheKeyIdentifiable
{
    /// <summary>
    ///     Represents the identifier of a WordleGuess object.
    /// </summary>
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    ///     Represents the ID of the player who made the guess in the Wordle game.
    /// </summary>
    /// <remarks>
    ///     The GuessedById property is of type ulong and is used to store the ID of the player who made the guess.
    /// </remarks>
    [Required]
    public ulong GuessedById { get; set; }

    /// <summary>
    ///     Represents a collection of letters used in the Wordle game.
    /// </summary>
    [MaxLength(8)]
    public List<WordleLetter> Letters { get; set; } = [];

    /// <summary>
    ///     Represents a word entered by a player in the Wordle game.
    /// </summary>
    public string Word => string.Concat(Letters.OrderBy(l => l.Position).Select(l => l.Letter));

    /// <summary>
    ///     Gets a value indicating whether the word entered by a player is correct in the Wordle game.
    /// </summary>
    /// <remarks>
    ///     The IsCorrect property is determined by checking the state of each letter in the word entered.
    ///     If all letters are in the correct state, this property returns true; otherwise, it returns false.
    /// </remarks>
    public bool IsCorrect => Letters.All(l => l.State is WordleLetterStateEnum.Correct);

    /// <summary>
    ///     Represents the unique identifier for a Wordle game.
    /// </summary>
    [Required]
    public int WordleId { get; set; }

    /// <summary>
    ///     The wordle this guess relates to
    /// </summary>
    [Required]
    public Wordle Wordle { get; set; } = new();

    public string GetCacheKey()
    {
        string serializedState = string.Join("-", this.Letters.Select(letter =>
            $"{letter.Position}:{letter.Letter}_{letter.State}"));
        string md5Hash = serializedState.ToMd5();

        return $"wordle:image:{md5Hash}";
    }
}