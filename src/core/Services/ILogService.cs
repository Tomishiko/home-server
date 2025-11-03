namespace core.Services;
using core.Models;

public interface ILogService
{
    Task NewLogAsync(Log log);
    IAsyncEnumerable<Log> GetAll(string? timeZone);
    IAsyncEnumerable<Log> GetPage(uint last, int perPage, string? timeZone);
    Task<int> SaveChangesAsync();
}
