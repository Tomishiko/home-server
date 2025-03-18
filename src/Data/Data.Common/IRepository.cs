namespace Data.Common;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    IEnumerable<T> GetAll();
}

