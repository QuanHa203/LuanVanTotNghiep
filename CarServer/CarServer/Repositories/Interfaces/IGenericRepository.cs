using CarServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarServer.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    DbSet<T> GetDbSet();
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T identity);
    Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate);

    Task SaveChangesAsync();
}
