using System.Collections.Immutable;
using core.Models;

namespace core.Services;


public interface ILogService : IBaseDataService
{
    IAsyncEnumerable<LogDto> GetAll(string? timeZone, CancellationToken ct);
    Task<ImmutableArray<LogDto>> GetPage(int perPage, string? timeZone, long lastId = default, DateTimeOffset lastTime = default);
    Task<int> AddNewLog(LogDto log);
    Task<int> AddNewLogs(params LogDto[] logs);
    Task<int> AddNewLogs(IEnumerable<LogDto> logs);
}
