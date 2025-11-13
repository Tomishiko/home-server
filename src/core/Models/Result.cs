namespace core.Models;

public enum ResultStatus{
    Success,
    Fail,
    Error
}
public record Result<T>(ResultStatus status,T? resultObject);
