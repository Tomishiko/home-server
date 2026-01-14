namespace core.Models;

public enum ResultStatus
{
    Success,
    Fail,
    Error
}
public record Result<T>(ResultStatus status, string? resultMsg = null, T? resultObject = default);
