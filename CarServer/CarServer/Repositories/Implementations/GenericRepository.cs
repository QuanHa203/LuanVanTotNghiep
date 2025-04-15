using CarServer.Databases;
using CarServer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarServer.Repositories.Implementations;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbSet<T> _dbSet;
    private readonly CarServerDbContext _dbContext;

    public GenericRepository(CarServerDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();        
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

    public DbSet<T> GetDbSet() =>  _dbSet;

    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<bool> IsExistAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
