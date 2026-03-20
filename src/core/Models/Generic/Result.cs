namespace core.Models.Generic;

public abstract record Result<T>
{
    public static implicit operator Result<T>(T value) => new Success<T>(value);
    public static implicit operator Result<T>(Error error) => new Failure<T>(error);
}
public sealed record Success<T>(T Value) : Result<T>;

public sealed record Failure<T>(Error Error) : Result<T>;

public sealed record Error(string Message, int? Code = null);
