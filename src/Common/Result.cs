namespace SocialLogin.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error is HasError)
            throw new ArgumentException("A successful result cannot carry an error.", nameof(error));

        if (!isSuccess && error is NoError)
            throw new ArgumentException("A failed result must carry an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, new NoError());
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public Result(T value) : base(true, new NoError())
    {
        _value = value;
    }

    public Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Value cannot be accessed when IsSuccess is false.");
}
