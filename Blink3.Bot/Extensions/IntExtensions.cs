namespace Blink3.Bot.Extensions;

public static class IntExtensions
{
    public static string GetStreakText(this int currentStreak)
    {
        return currentStreak > 0 ? $"🔥 {currentStreak.ToSuperscript()}" : string.Empty;
    }

    private static string ToSuperscript(this int number)
    {
        string normalNumbers = "0123456789";
        string superscriptNumbers = "⁰¹²³⁴⁵⁶⁷⁸⁹";

        char[] result = number.ToString().Select(c =>
        {
            int index = normalNumbers.IndexOf(c);
            return superscriptNumbers[index];
        }).ToArray();

        return new string(result);
    }
}