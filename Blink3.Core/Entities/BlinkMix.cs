using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Blink3.Core.Base;
using Blink3.Core.Enums;
using Blink3.Core.Extensions;

namespace Blink3.Core.Entities;

public class BlinkMix : GameBase
{
    public override GameType Type => GameType.BlinkMix;
    
    private readonly string _solution = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string Solution
    {
        get => _solution;
        init
        {
            _solution = value ?? throw new ArgumentNullException(nameof(value));
            ShuffledSolution = _solution.Shuffle();
        }
    }

    [Required]
    [MaxLength(10)]
    public string ShuffledSolution { get; private set; } = string.Empty;
    
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