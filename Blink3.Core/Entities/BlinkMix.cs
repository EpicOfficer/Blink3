using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Blink3.Core.Base;
using Blink3.Core.Enums;

namespace Blink3.Core.Entities;

public class BlinkMix : GameBase
{
    public override GameType Type => GameType.BlinkMix;

    [Required]
    [MaxLength(10)]
    public string Solution { get; set; } = string.Empty;
    
    /// <summary>
    /// Returns a shuffled version of the Solution.
    /// </summary>
    public string GetShuffledSolution()
    {
        if (string.IsNullOrWhiteSpace(Solution))
            throw new InvalidOperationException("Solution is not set.");

        return new string(Solution.OrderBy(_ => Random.Shared.Next()).ToArray());
    }
    
    /// <summary>
    ///     Compares the provided user answer with the solution, ignoring case and cultural variations.
    /// </summary>
    public bool IsCorrectSolution(string guess)
    {
        if (string.IsNullOrWhiteSpace(guess))
            return false;

        return string.Compare(Solution, guess,
            CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}