using System.Globalization;

namespace Blink3.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Converts the input string to an unsigned long (ulong) value.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The converted ulong value.</returns>
    /// <exception cref="System.FormatException">Thrown if the input string has an invalid format for ulong conversion.</exception>
    public static ulong ToUlong(this string input)
    {
        if (!ulong.TryParse(input, out ulong result)) throw new FormatException("Invalid format for ulong conversion");

        return result;
    }

    /// <summary>
    ///     Converts a string to title case.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>The converted string in title case.</returns>
    public static string ToTitleCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(str);
    }

    /// <summary>
    ///     Converts a string to sentence case.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>The converted string in sentence case.</returns>
    public static string ToSentenceCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;
        return char.ToUpper(str[0]) + str[1..].ToLower();
    }
}