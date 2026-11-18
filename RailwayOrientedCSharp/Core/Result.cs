namespace RailwayOrientedCSharp.Core;

/// <summary>
/// Outcome of an operation that either succeeded or failed with one or more error messages.
/// </summary>
/// <remarks>
/// The point of this type is to make failure an ordinary return value. Business failures — a
/// missing customer, a declined card — are expected outcomes, so they travel as data instead of as
/// exceptions, which keeps them visible in method signatures and cheap to compose.
/// <para>Errors are a list rather than a single string so that parallel validation can report
/// everything that is wrong at once; a sequential pipeline will only ever carry one.</para>
/// <para>Construction goes through the static factories below, never a public constructor, so a
/// result cannot be built in a contradictory state (success with errors, failure without).</para>
/// </remarks>
public class Result
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Convenience inverse of <see cref="IsSuccess"/>, for readability at call sites.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The error messages. Empty on success.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Protected so results can only be created through the factory methods.</summary>
    /// <param name="isSuccess">Whether this result represents success.</param>
    /// <param name="errors">The errors to carry; empty for success.</param>
    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        // Materialised immediately: a lazy sequence could otherwise be enumerated repeatedly, or
        // change underneath the result after it was created.
        Errors = [.. errors];
    }

    /// <summary>A success carrying no value.</summary>
    public static Result Success() => new(true, Array.Empty<string>());

    /// <summary>A failure carrying a single error.</summary>
    /// <param name="error">The error message.</param>
    public static Result Failure(string error) => new(false, [error]);

    /// <summary>A failure carrying several errors — the shape <c>Combine</c> produces.</summary>
    /// <param name="errors">The error messages.</param>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    // The generic factories below exist so call sites can write Result.Success(x) and
    // Result.Failure<T>(...) uniformly, instead of naming Result<T> explicitly each time.

    /// <summary>A success carrying a value.</summary>
    /// <typeparam name="T">The type of the carried value.</typeparam>
    /// <param name="value">The value to carry.</param>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>A typed failure carrying a single error.</summary>
    /// <typeparam name="T">The value type the result would have carried.</typeparam>
    /// <param name="error">The error message.</param>
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);

    /// <summary>A typed failure carrying several errors.</summary>
    /// <typeparam name="T">The value type the result would have carried.</typeparam>
    /// <param name="errors">The error messages.</param>
    public static Result<T> Failure<T>(IEnumerable<string> errors) => Result<T>.Failure(errors);
}

/// <summary>
/// A <see cref="Result"/> that carries a value when it succeeds.
/// </summary>
/// <typeparam name="T">Type of the value carried on success.</typeparam>
/// <remarks>
/// Deriving from <see cref="Result"/> means a typed result can be passed anywhere an untyped one is
/// expected — convenient, but it is also why the <c>Failure</c> factories here need the
/// <c>new</c> keyword: they intentionally hide the base versions so that, inside a
/// <c>Result&lt;T&gt;</c> context, the typed overload wins.
/// </remarks>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// The carried value.
    /// </summary>
    /// <exception cref="InvalidOperationException">The result is a failure.</exception>
    /// <remarks>
    /// Throwing rather than returning default is deliberate: reading the value of a failed result
    /// is a programming error, and it should surface loudly at the point of the mistake. In normal
    /// use the combinators read this only after checking <see cref="Result.IsSuccess"/>, so it
    /// never throws.
    /// </remarks>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>Protected so results can only be created through the factory methods.</summary>
    /// <param name="value">The value to carry, or default for a failure.</param>
    /// <param name="isSuccess">Whether this result represents success.</param>
    /// <param name="errors">The errors to carry; empty for success.</param>
    protected Result(T? value, bool isSuccess, IEnumerable<string> errors) : base(isSuccess, errors)
    {
        _value = value;
    }

    /// <summary>A success carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value to carry.</param>
    public static Result<T> Success(T value) => new(value, true, Array.Empty<string>());

    /// <summary>A failure carrying a single error and no value.</summary>
    /// <param name="error">The error message.</param>
    public static new Result<T> Failure(string error) => new(default, false, new[] { error });

    /// <summary>A failure carrying several errors and no value.</summary>
    /// <param name="errors">The error messages.</param>
    public static new Result<T> Failure(IEnumerable<string> errors) => new(default, false, errors);
}
