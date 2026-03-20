namespace Data.Shared;

using Data.Core;
using Microsoft.EntityFrameworkCore;

//public class Repository<T> : IRepository<T> where T : BaseEntity
//{
//    protected ApplicationDbContext _context;
//    protected DbSet<T> _dbSet;
//
//    public Repository(ApplicationDbContext context)
//    {
//        _context = context ?? throw new ArgumentNullException(nameof(context));
//        _dbSet = _context.Set<T>();
//    }
//
//    public virtual Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();
//
//    public virtual void Add(T entity) => _dbSet.Add(entity);
//
//    public virtual void Update(T entity)
//    {
//        var entry = _context.Entry(entity);
//        if (entry.State == EntityState.Detached)
//            _dbSet.Attach(entity);
//
//        entry.State = EntityState.Modified;
//    }
//
//    public virtual void Delete(T entity) => _dbSet.Remove(entity);
//
//    public virtual Task<T?> GetByIdAsync(uint id) => _dbSet.FindAsync(id).AsTask();
//
//    public virtual Task<List<T>> ToListAsync(CancellationToken cancellationToken) => _dbSet.ToListAsync(cancellationToken);
//
//    public virtual IEnumerable<T> GetAll() => _dbSet.AsEnumerable();
//
//    public virtual IAsyncEnumerable<T> GetAllAsync() => _dbSet.AsAsyncEnumerable();
//
//    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();
//
//    public virtual Task<int> SaveContextAsync() => _context.SaveChangesAsync();
//
//    public virtual int SaveContext() => _context.SaveChanges();
//
//}

