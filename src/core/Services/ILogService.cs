namespace core.Services;
using core.Models;

public interface ILogService
{
    Task NewLogAsync(Log log);
    IAsyncEnumerable<Log> GetAll();
    IAsyncEnumerable<Log> GetPage(uint last, int perPage);
    Task<int> SaveChangesAsync();
}
