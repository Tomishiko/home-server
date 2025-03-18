namespace Data.Core;
using Data.Common;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    protected ApplicationDbContext _context;
    protected DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();

    public void Update(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
            _dbSet.Attach(entity);

        entry.State = EntityState.Modified;
    }

    public void Delete(T entity) => _dbSet.Remove(entity);

    public Task<T> GetByIdAsync(int id) => _dbSet.FindAsync(id).AsTask();

    public Task<List<T>> GetAllAsync(CancellationToken cancellationToken) => _dbSet.ToListAsync(cancellationToken);

    public IEnumerable<T> GetAll() => _dbSet.ToList();
}

