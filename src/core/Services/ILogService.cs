namespace core.Services;
using core.Models;

public interface ILogService
{
    void NewLog(Log log);
    IEnumerable<Log> GetAll();
}
