using Blink3.Core.Entities;

namespace Blink3.Core.Extensions;

public static class BlinkMixExtensions
{
    public static int GetScore(this BlinkMix game)
    {
        const int basePoints = 5;
        int bonusPoints = Math.Max(0, game.Solution.Length - 4);
        return basePoints + bonusPoints;
    }
}