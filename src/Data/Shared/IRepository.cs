using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Data.Shared;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    IEnumerable<T> GetAll();
    IQueryable<T> Query();
    IIncludableQueryable<T,TProperty> Include<TProperty>(Expression<Func<T,TProperty>> path)where TProperty:class;
}

