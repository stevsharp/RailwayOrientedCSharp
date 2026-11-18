
namespace RailwayOrientedCSharp.Core;

public static class ResultExtensions
{
    public static Result Combine(params Result[] results)
    {
        var errors = results.Where(r => r.IsFailure)
            .SelectMany(r => r.Errors).ToList();
        
        return errors.Count > 0 ? Result.Failure(errors) : Result.Success();
    }

    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> func) =>
        result.IsFailure ? Result.Failure<TOut>(result.Errors) : func(result.Value);

    public static async Task<Result<TOut>> BindAsync<TOut>(
        this Result result,
        Func<Task<Result<TOut>>> func) =>
        result.IsFailure ? Result.Failure<TOut>(result.Errors) : await func();

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask;
        return result.IsFailure ? Result.Failure<TOut>(result.Errors) : await func(result.Value);
    }

    public static async Task<Result<T>> BindIfAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> condition,
        Func<T, Task<Result<T>>> conditionalStep)
    {
        var result = await resultTask;
        
        if (result.IsFailure) 
            return result;

        return condition(result.Value) ? await conditionalStep(result.Value) : result;
    }
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        string errorMessage)
    {
        var result = await resultTask;
        
        if (result.IsFailure) 
            return result;

        return predicate(result.Value) ? result : Result.Failure<T>(errorMessage);
    }

    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Func<T, string> errorFactory)
    {
        var result = await resultTask;
        if (result.IsFailure) return result;
        return predicate(result.Value) ? result : Result.Failure<T>(errorFactory(result.Value));
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> func)
    {
        var result = await resultTask;
        return result.IsFailure ? Result.Failure<TOut>(result.Errors) : Result.Success(func(result.Value));
    }

    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action)
    {
        var result = await resultTask;
        if (result.IsSuccess) action(result.Value);
        return result;
    }
}
