namespace Blink3.Core.Models;

/// <summary>
///     Represents the result of an operation, which can either be successful or failed.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> where T : class
{
    /// <inheritdoc cref="Result{T}" />
    protected Result(bool isSuccess, string? error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation represented by the Result object was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    ///     Represents the result of an operation, which may contain a value or an error message.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public string? Error { get; private set; }

    /// <summary>
    ///     Represents the result value of an operation in the <see cref="Result{T}" /> class.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public T? Value { get; private set; }

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be stored in the result.</param>
    /// <returns>A successful result with the specified value.</returns>
    public static Result<T> Ok(T? value)
    {
        return new Result<T>(true, null, value);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Result{T}" /> class indicating a failure.
    /// </summary>
    /// <param name="error">The error message associated with the failure.</param>
    /// <returns>A new instance of the <see cref="Result{T}" /> class.</returns>
    public static Result<T> Fail(string error)
    {
        return new Result<T>(false, error, null);
    }
}