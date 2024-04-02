using Blink3.Core.Enums;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Blink3.Core.Entities;

/// <summary>
///     Represents a letter used in the Wordle Game.
/// </summary>
public class WordleLetter
{
    /// <summary>
    ///     The position of this letter in the wordle.
    /// </summary>
    public int Position { get; set; }
    
    /// <summary>
    ///     Represents a letter used in the Wordle Game.
    /// </summary>
    public char Letter { get; set; }

    /// <summary>
    ///     Represents the state of the letter used in the Wordle Game.
    /// </summary>
    public WordleLetterStateEnum State { get; set; }
}