using BookShelves.Shared.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookShelves.Shared.Data.Bases;

public class GenericRepository<TDbContext, TEntity>(TDbContext context) : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _context = context;
    private DbSet<TEntity> DbSet => _context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await DbSet.ToListAsync();
    
    public async Task<IEnumerable<TEntity>> GetAllReadOnlyAsync() => await DbSet.AsNoTracking().ToListAsync();

    public async Task<TEntity?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
    
    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) => 
        await DbSet.Where(predicate).ToListAsync();

    public async Task<IEnumerable<TEntity>> FindReadOnlyAsync(Expression<Func<TEntity, bool>> predicate) => 
        await DbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task AddAsync(TEntity entity) => await DbSet.AddAsync(entity);
    
    public Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, bool readOnly = false)
    {
        IQueryable<TEntity> query;
        if (readOnly)
        {
            query = DbSet.AsNoTracking();
        }
        else
        {
            query = DbSet;
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
