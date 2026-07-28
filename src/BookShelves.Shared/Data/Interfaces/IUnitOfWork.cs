using Microsoft.EntityFrameworkCore;

namespace BookShelves.Shared.Data.Interfaces;

public interface IUnitOfWork<TContext> : IAsyncDisposable
    where TContext : DbContext
{
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
