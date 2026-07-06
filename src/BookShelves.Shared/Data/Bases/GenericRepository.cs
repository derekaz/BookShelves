using BookShelves.Shared.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookShelves.Shared.Data.Bases;

public class GenericRepository<TDbContext, TEntity> : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _context; // = context;
    private readonly DbSet<TEntity> _dbSet; // => _context.Set<TEntity>();

    public GenericRepository(TDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }


    public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<IEnumerable<TEntity>> GetAllReadOnlyAsync() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<TEntity?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<IEnumerable<TEntity>> FindReadOnlyAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

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

    public async Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool readOnly = false)
    {
        IQueryable<TEntity> query;
        if (readOnly)
        {
            query = _dbSet.AsNoTracking();
        }
        else
        {
            query = _dbSet;
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}
