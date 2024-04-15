using Blink3.Core.Models;

namespace Blink3.Core.Extensions;

public static class ResultExtensions
{
    /// <summary>
    ///     Retrieves the value from the result if it is successful and not null; otherwise, throws an exception.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result to get the value from.</param>
    /// <returns>The value from the result, if it is successful and not null.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the result is not successful or the value is null.
    /// </exception>
    public static T SafeValue<T>(this Result<T> result) where T : class
    {
        if (!result.IsSuccess) throw new InvalidOperationException(result.Error);

        return result.Value ?? throw new InvalidOperationException("Unexpected null result while executing SafeValue.");
    }
}