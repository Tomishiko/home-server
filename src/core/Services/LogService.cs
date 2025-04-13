namespace core.Services;
using core.Models;
using Data.Shared;
using Data.Models;
using Microsoft.EntityFrameworkCore;


public class LogService : ILogService
{
    IRepository<LogsEntity> _logsRepo;

    public LogService(IRepository<LogsEntity> logsRepo)
    {
        _logsRepo = logsRepo;
    }

    public IEnumerable<Log> GetAll()
    {
        return _logsRepo.Query()
            .Include("User")
            .Select(u => new Log(u.Event, u.Time,u.User.Uname));
    }
    public void NewLog(Log log, uint? user_id)
    {
        _logsRepo.Add(new LogsEntity
        {
            user_id = user_id,
            Time = log.Time,
            Event = log.Event
        });
        _logsRepo.SaveContext();
    }

}
