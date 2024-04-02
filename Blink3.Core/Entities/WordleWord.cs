// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a word entered by a player in the Wordle game.
/// </summary>
public class WordleWord
{
    /// <summary>
    ///     Represents the identifier of the player who made a guess in the WordleWord class.
    /// </summary>
    /// <value>
    ///     The identifier of the player.
    /// </value>
    public ulong GuessedById { get; set; }

    /// <summary>
    ///     Represents a collection of letters used in the Wordle game.
    /// </summary>
    public List<WordleLetter> Letters { get; set; } = [];
}