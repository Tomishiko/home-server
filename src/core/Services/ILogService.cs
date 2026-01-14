namespace core.Services;

using System.Collections.Immutable;
using core.Models;

public interface ILogService : IBaseDataService
{
    Task NewLogAsync(Log log);
    IAsyncEnumerable<Log> GetAll(string? timeZone);
    Task<ImmutableArray<Log>> GetPage(int perPage, string? timeZone, uint lastId = default, DateTime lastTime = default);
}
