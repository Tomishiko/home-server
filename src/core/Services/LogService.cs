namespace core.Services;
using core.Models;
using Data.Models;
using Data.Core;

public class LogService : BaseDataService, ILogService
{

    public LogService(ApplicationDbContext context) : base(context) { }

    public IEnumerable<Log> GetAll()
    {
        return _context.Logs.Select(l => new Log(l.Event, l.Time, l.Uname));
    }

    public async Task NewLogAsync(Log log)
    {
        await _context.Logs.AddAsync(new LogsEntity {
            Uname = log.Uname,
            Time = log.Time,
            Event = log.Event
        });
    }

}
