namespace core.Services;

using System.Collections.Immutable;
using core.Models;

public interface ILogService : IBaseDataService
{
    Task NewLogAsync(LogDto log);
    IAsyncEnumerable<LogDto> GetAll(string? timeZone);
    Task<ImmutableArray<LogDto>> GetPage(int perPage, string? timeZone, long lastId = default, DateTimeOffset lastTime = default);
}
