namespace core.Services;

using core.Interfaces;

public class BaseDataService : IBaseDataService
{
    protected readonly IApplicationDbContext _context;

    public BaseDataService(IApplicationDbContext context)
    {
        _context = context;
    }
    public virtual Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();

    }
}
