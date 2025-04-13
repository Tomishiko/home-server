namespace core.Services;
using core.Models;

public interface ILogService
{

    IEnumerable<Log> GetAll();
}
