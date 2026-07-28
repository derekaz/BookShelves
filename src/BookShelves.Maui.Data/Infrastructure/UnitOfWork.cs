using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data.Infrastructure;

// DataAccess/UnitOfWork.cs
public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext // (IDbContextFactory<BookShelvesDbContext> dbFactory, BookShelvesDbContext context) : IUnitOfWork<LocalBook>
{
    protected TContext Context { get; }
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(IDbContextFactory<TContext> contextFactory)
    {
        Context = contextFactory.CreateDbContext();
    }

    public IRepository<T> GetRepository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<TContext, T>(Context);
        }
        return (IRepository<T>)_repositories[type];
    }

    //_context = await _dbFactory.CreateDbContextAsync();

    //public async Task ResetContextAsync()
    //{
    //    if (_context != null)
    //    {
    //        await _context.DisposeAsync();
    //    }
    //    _context = await _dbFactory.CreateDbContextAsync();

    //    _localBooks = null;
    //}

    //private BookShelvesDbContext EnsureContext()
    //{
    //    if (_context == null)
    //    {
    //        ResetContextAsync().Wait();
    //    }

    //    return _context!;
    //}

    //public IRepository<LocalBook> LocalBooks => _localBooks ??= new GenericRepository<BookShelvesDbContext, LocalBook>(_context ?? EnsureContext());
    // Initialize other repositories similarly

    public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();

    public async ValueTask DisposeAsync() => await Context.DisposeAsync();
}
