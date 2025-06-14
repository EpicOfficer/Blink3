using System.Globalization;
using System.Security.Cryptography;
using System.Text;

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
    ///     Tries to convert the input string to an unsigned long (ulong) value.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <param name="result">
    ///     When this method returns, contains the converted ulong value if the conversion succeeded, or zero
    ///     if the conversion failed.
    /// </param>
    /// <returns>True if the conversion succeeded; otherwise, false.</returns>
    public static bool TryToUlong(this string input, out ulong result)
    {
        return ulong.TryParse(input, out result);
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

    /// <summary>
    ///     Truncates a string to a specified maximum length.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length of the truncated string.</param>
    /// <returns>The truncated string.</returns>
    public static string TruncateTo(this string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input.Length <= maxLength ? input : string.Concat(input.AsSpan(0, maxLength - 3), "...");
    }

    /// <summary>
    ///     Converts a string to its MD5 hash value.
    /// </summary>
    /// <param name="str">The input string to convert.</param>
    /// <returns>The MD5 hash value of the input string.</returns>
    public static string ToMd5(this string str)
    {
        // Use input string to calculate MD5 hash
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(str);
        byte[] hashBytes = MD5.HashData(inputBytes);

        // Convert the byte array to hexadecimal string
        StringBuilder sb = new();
        foreach (byte t in hashBytes) sb.Append(t.ToString("x2"));
        return sb.ToString();
    }

    /// <summary>
    ///     Finds all indexes of a specified character in a string.
    /// </summary>
    /// <param name="str">The input string to search in.</param>
    /// <param name="value">The character to find the indexes of.</param>
    /// <returns>A list of integers representing the indexes at which the character was found.</returns>
    public static List<int> AllIndexesOf(this string str, char value)
    {
        List<int> indexes = [];
        int index = 0;
        while ((index = str.IndexOf(value, index)) != -1)
        {
            indexes.Add(index);
            index++;
        }

        return indexes;
    }
}