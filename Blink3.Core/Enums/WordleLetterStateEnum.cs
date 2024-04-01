namespace Blink3.Core.Enums;

/// <summary>
///     Represents the state of a letter in the Wordle game.
/// </summary>
public enum WordleLetterStateEnum
{
    /// <summary>
    ///     This state indicates that the letter is not present in the wordle.
    /// </summary>
    Incorrect,

    /// <summary>
    ///     This state indicates that the letter is present in the wordle, but is not in the correct position.
    /// </summary>
    Misplaced,

    /// <summary>
    ///     This state indicates that the letter is present in the wordle, and is in the correct position
    /// </summary>
    Correct
}