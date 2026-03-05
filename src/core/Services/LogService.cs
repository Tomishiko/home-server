using core.Models;
using core.Domain;
using core.Extensions;
using core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace core.Services;

public class LogService : BaseDataService, ILogService
{

    public LogService(IApplicationDbContext context) : base(context) { }

    public IAsyncEnumerable<LogDto> GetAll(string? timeZone)
    {
        return _context.Logs
            .AsNoTracking()
            .Select(l => new LogDto(
                l.Id,
                l.Event,
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(l.Time,
                                                           timeZone ?? "UTC"),
                l.Uname))
            .AsAsyncEnumerable();
    }

    public async Task NewLogAsync(LogDto log)
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

    public async Task<ImmutableArray<LogDto>> GetPage(int perPage,
                                                      string? timeZone,
                                                      long lastId = default,
                                                      DateTimeOffset lastTime = default)
    {

        IQueryable<LogsEntity> query = _context.Logs;

        // Apply where filter if its not first page
        if (lastId > 0)
        {

            //Console.WriteLine($"{lastId}");
            //Console.WriteLine($"{lastTime}");
            lastTime = lastTime.ToUniversalTime();
            query = query.Where(l => l.Time < lastTime ||
                       (l.Time == lastTime && l.Id < lastId));
        }

        var test = await query.OrderByDescending(l => l.Time)
                          .ThenByDescending(l => l.Id)
                          .Take(perPage)
                          .AsNoTracking()
                          .Select(l =>
                              new LogDto(l.Id,
                                         l.Event,
                                         TimeZoneInfo.ConvertTimeBySystemTimeZoneId(l.Time,
                                                                                    timeZone ?? "UTC"),
                                         l.Uname))
                          .ToImmutableArrayAsync();
        foreach (var entry in test)
        {
            Console.WriteLine(entry);
        }
        return test;

    }

}
