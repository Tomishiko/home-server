namespace core.Services;
using core.Models;
using Data.Shared;
using Data.Models;


public class LogService : ILogService
{
    IRepository<LogsEntity> _logsRepo;

    public LogService(IRepository<LogsEntity> logsRepo)
    {
        _logsRepo = logsRepo;
    }

    public IEnumerable<Log> GetAll()
    {
        return _logsRepo.Query().Select(l => new Log(l.Event, l.Time,l.Uname));
    }
    public void NewLog(Log log)
    {
        _logsRepo.Add(new LogsEntity
        {
            Uname = log.Uname,
            Time = log.Time,
            Event = log.Event
        });
        //_logsRepo.SaveContext();
    }
    public Task<int> SaveChanges(){
        return _logsRepo.SaveContextAsync();
    }

}
