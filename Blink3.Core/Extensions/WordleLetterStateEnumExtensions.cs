using Blink3.Core.Enums;

namespace Blink3.Core.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="WordleLetterStateEnum" /> enum.
/// </summary>
public static class WordleLetterStateEnumExtensions
{
    /// <summary>
    ///     Gets the icon associated with the given Wordle letter state.
    /// </summary>
    /// <param name="state">The Wordle letter state.</param>
    /// <returns>The icon character representing the given state.</returns>
    public static char GetIcon(this WordleLetterStateEnum state)
    {
        return state switch
        {
            WordleLetterStateEnum.Correct => '\uE002',
            WordleLetterStateEnum.Misplaced => '\uE001',
            _ => '\uE000'
        };
    }
}