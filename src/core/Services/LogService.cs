namespace core.Services;
using core.Models;
using Data.Models;
using Data.Core;
using Microsoft.EntityFrameworkCore;

public class LogService : BaseDataService, ILogService
{

    public LogService(ApplicationDbContext context) : base(context) { }

    public IAsyncEnumerable<Log> GetAll(string? timeZone)
    {
        return _context.Logs
            .Select(l => new Log(
                l.Id,
                l.Event,
                AdjustForTimezone(l, timeZone),
                l.Uname))
            .AsAsyncEnumerable();
    }

    public async Task NewLogAsync(Log log)
    {
        var time = log.Time.ToUniversalTime();
        _context.Logs.Add(new LogsEntity
        {
            Uname = log.Uname,
            Time = time,
            Event = log.Event
        });
        await SaveChangesAsync();
    }

    public IAsyncEnumerable<Log> GetPage(uint last, int perPage, string? timeZone)
    {

        return _context.Logs.OrderBy(l => l.Id)
                            .Where(l => l.Id > last)
                            .Take(perPage)
                            .Select(l => new Log(l.Id,
                                                 l.Event,
                                                 AdjustForTimezone(l, timeZone),
                                                 l.Uname))
                            .AsAsyncEnumerable();
    }
    static private DateTime AdjustForTimezone(LogsEntity l, string? timeZone)
    {
        if (string.IsNullOrEmpty(timeZone)) return l.Time;

        TimeZoneInfo? tzInfo;
        TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out tzInfo);
        if (tzInfo is null) return l.Time;

        return TimeZoneInfo.ConvertTimeFromUtc(l.Time, tzInfo);

    }

}
