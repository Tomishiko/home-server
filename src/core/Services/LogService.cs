namespace core.Services;
using core.Models;
using Data.Models;
using Data.Core;
using Microsoft.EntityFrameworkCore;

public class LogService : BaseDataService, ILogService
{

    public LogService(ApplicationDbContext context) : base(context) { }

    public IAsyncEnumerable<Log> GetAll()
    {
        return _context.Logs
            .Select(l => new Log(l.Id,l.Event, l.Time, l.Uname))
            .AsAsyncEnumerable();
    }

    public async Task NewLogAsync(Log log)
    {
        await _context.Logs.AddAsync(new LogsEntity
        {
            Uname = log.Uname,
            Time = log.Time,
            Event = log.Event
        });
    }
    public IAsyncEnumerable<Log> GetPage(uint last, int perPage)
    {

        return _context.Logs.OrderBy(l=>l.Id)
                            .Where(l=>l.Id > last)
                            .Take(perPage)
                            .Select(l => new Log(l.Id,l.Event, l.Time, l.Uname))
                            .AsAsyncEnumerable();
    }

}
