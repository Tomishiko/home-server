namespace core.Models.Generic;

public abstract record Result<T>
{
    public bool IsSuccess => this is Success<T>;
    public bool IsFailure => this is Failure<T>;

    public T Value => this is Success<T> success
        ? success.Data
        : throw new InvalidOperationException("Cannot access Value of a failure result.");

    public Error Error => this is Failure<T> failure
        ? failure.Details
        : throw new InvalidOperationException("Cannot access Error of a success result.");

    public static implicit operator Result<T>(T value) => new Success<T>(value);
    public static implicit operator Result<T>(Error error) => new Failure<T>(error);
}
public sealed record Success<T>(T Data) : Result<T>;

public sealed record Failure<T>(Error Details) : Result<T>;

public sealed record Error(string Message, int? Code = null);
