namespace Nalam360.Platform.Core.Results;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail
/// </summary>
/// <typeparam name="T">The type of the value returned on success</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access Value of a failed result");

    public Error Error => IsFailure 
        ? _error! 
        : throw new InvalidOperationException("Cannot access Error of a successful result");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = default;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
    public static Result<T> Failure(string code, string message) => new(new Error(code, message));

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Error);

    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess, 
        Func<Error, Task<TResult>> onFailure)
        => IsSuccess ? await onSuccess(Value) : await onFailure(Error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
        => IsSuccess ? Result<TOut>.Success(mapper(Value)) : Result<TOut>.Failure(Error);

    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper)
        => IsSuccess ? Result<TOut>.Success(await mapper(Value)) : Result<TOut>.Failure(Error);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
        => IsSuccess ? binder(Value) : Result<TOut>.Failure(Error);

    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder)
        => IsSuccess ? await binder(Value) : Result<TOut>.Failure(Error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation without a return value
/// </summary>
public readonly struct Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure 
        ? _error! 
        : throw new InvalidOperationException("Cannot access Error of a successful result");

    private Result(bool isSuccess, Error? error = null)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new Error(code, message));

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);

    public static implicit operator Result(Error error) => Failure(error);
}
