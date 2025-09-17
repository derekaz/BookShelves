using BookShelves.Maui.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data;

public class LocalRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly BookShelvesDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public LocalRepository(BookShelvesDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<TEntity?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);
    public Task UpdateAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(TEntity entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
