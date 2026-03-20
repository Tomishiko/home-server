using core.Models;
using core.Domain;
using core.Extensions;
using core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace core.Services;

public class LogService : BaseDataService, ILogService
{

    public LogService(IApplicationDbContext context) : base(context) { }

    public async IAsyncEnumerable<LogDto> GetAll(string? timeZone,
                                                 [EnumeratorCancellation] CancellationToken ct = default)
    {
        var stream = _context.Logs
            .AsNoTracking()
            .AsAsyncEnumerable()
            .WithCancellation(ct);

        await foreach (LogsEntity l in stream)
        {
            yield return new LogDto(
                l.Id,
                l.Event,
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(l.Time,
                                                           timeZone ?? "UTC"),
                l.Uname);
        }
    }

    public Task<int> AddNewLog(LogDto log)
    {
        StageNewLog(log);
        return SaveChangesAsync();
    }
    public Task<int> AddNewLogs(IEnumerable<LogDto> logs)
    {
        foreach (var log in logs)
        {
            StageNewLog(log);
        }
        return SaveChangesAsync();
    }

    public Task<int> AddNewLogs(params LogDto[] logs)
    {
        foreach (var log in logs)
        {
            StageNewLog(log);
        }
        return SaveChangesAsync();
    }

    public Task<ImmutableArray<LogDto>> GetPage(int perPage,
                                                string? timeZone,
                                                long lastId = default,
                                                DateTimeOffset lastTime = default)
    {

        IQueryable<LogsEntity> query = _context.Logs;

        // Apply where filter if its not first page
        if (lastId > 0)
        {

            lastTime = lastTime.ToUniversalTime();
            query = query.Where(l => l.Time < lastTime ||
                       (l.Time == lastTime && l.Id < lastId));
        }

        return query.OrderByDescending(l => l.Time)
                          .ThenByDescending(l => l.Id)
                          .Take(perPage)
                          .AsNoTracking()
                          .Select(l =>
                              new LogDto(l.Id,
                                         l.Event,
                                         TimeZoneInfo.ConvertTimeBySystemTimeZoneId(l.Time,
                                                                                    timeZone ?? "UTC"),
                                         l.Uname))
                          .ToImmutableArrayAsync(10);

    }
    private void StageNewLog(LogDto log)
    {

        var time = log.Time.ToUniversalTime();

        _context.Logs.Add(new LogsEntity
        {
            Uname = log.Uname,
            Time = time,
            Event = log.Event
        });
    }

}
