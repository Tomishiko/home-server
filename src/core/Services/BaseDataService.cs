namespace core.Services;
using Data.Core;

public class BaseDataService : IBaseDataService
{
    protected readonly ApplicationDbContext _context;

    public BaseDataService(ApplicationDbContext context)
    {
        _context = context;
    }
    public virtual Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();

    }
}
