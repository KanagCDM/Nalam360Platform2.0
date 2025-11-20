namespace Nalam360.Platform.Core.Results;

/// <summary>
/// Either monad representing a value that can be one of two types
/// </summary>
public readonly struct Either<TLeft, TRight>
{
    private readonly TLeft? _left;
    private readonly TRight? _right;

    public bool IsLeft { get; }
    public bool IsRight => !IsLeft;

    public TLeft Left => IsLeft 
        ? _left! 
        : throw new InvalidOperationException("Cannot access Left of a Right value");

    public TRight Right => IsRight 
        ? _right! 
        : throw new InvalidOperationException("Cannot access Right of a Left value");

    private Either(TLeft left)
    {
        IsLeft = true;
        _left = left;
        _right = default;
    }

    private Either(TRight right)
    {
        IsLeft = false;
        _left = default;
        _right = right;
    }

    public static Either<TLeft, TRight> FromLeft(TLeft left) => new(left);
    public static Either<TLeft, TRight> FromRight(TRight right) => new(right);

    public TResult Match<TResult>(Func<TLeft, TResult> onLeft, Func<TRight, TResult> onRight)
        => IsLeft ? onLeft(Left) : onRight(Right);

    public void Match(Action<TLeft> onLeft, Action<TRight> onRight)
    {
        if (IsLeft) onLeft(Left);
        else onRight(Right);
    }

    public static implicit operator Either<TLeft, TRight>(TLeft left) => FromLeft(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => FromRight(right);
}
