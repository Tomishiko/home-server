namespace core.Services;
using core.Models;

public interface ILogService
{
    Task NewLogAsync(Log log);
    IEnumerable<Log> GetAll();
    Task<int> SaveChangesAsync();
}
