namespace Data.Shared;

using Data.Models;


public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<T?> GetByIdAsync(uint id);
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default(CancellationToken));
    IEnumerable<T> GetAll();
    IAsyncEnumerable<T> GetAllAsync();
    IQueryable<T> Query();
    Task<int> SaveContextAsync();
    int SaveContext();

}

