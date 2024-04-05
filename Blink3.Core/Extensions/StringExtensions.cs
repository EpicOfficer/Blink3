using System;

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
}