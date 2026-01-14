using core.Models;
using Data.Models;
using Data.Core;
using Data.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace core.Services;

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

    public async Task<ImmutableArray<Log>> GetPage(int perPage, string? timeZone, uint lastId = default, DateTime lastTime = default)
    {

        //return _context.Logs.OrderByDescending(l => l.Time)
        //                    .Where(l => l.Id > lastId)
        //                    .Take(perPage)
        //                    .Select(l => new Log(l.Id,
        //                                         l.Event,
        //                                         AdjustForTimezone(l, timeZone),
        //                                         l.Uname))
        //                    .AsAsyncEnumerable();
        IQueryable<LogsEntity> query = _context.Logs;

        //For first page case
        if (lastId > 0)
        {

            lastTime = lastTime.ToUniversalTime();
            query = query.Where(l => l.Time < lastTime ||
                       (l.Time == lastTime && l.Id < lastId));
        }

        return await query.OrderByDescending(l => l.Time)
                          .ThenByDescending(l => l.Id)
                          .Take(perPage)
                          .AsNoTracking()
                          .Select(l => new Log(l.Id,
                                               l.Event,
                                               AdjustForTimezone(l, timeZone),
                                               l.Uname))
                          .ToImmutableArrayAsync();

    }
    static private DateTime AdjustForTimezone(LogsEntity l, string? timeZone)
    {
        if (string.IsNullOrEmpty(timeZone)) return l.Time;

        TimeZoneInfo? tzInfo;
        TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out tzInfo);

        if (tzInfo is null)
        {
            return l.Time;
        }

        return TimeZoneInfo.ConvertTimeFromUtc(l.Time, tzInfo);

    }

}
