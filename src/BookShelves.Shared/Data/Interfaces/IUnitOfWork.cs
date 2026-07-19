using Microsoft.EntityFrameworkCore;

namespace BookShelves.Shared.Data.Interfaces;

//public interface IUnitOfWork<TLocalBook> : IDisposable
//    where TLocalBook : class, IBook
//{
//    IRepository<TLocalBook> LocalBooks { get; }
//    // Add other repository properties
//    Task<int> CompleteAsync();
//}

public interface IUnitOfWork<TContext> : IAsyncDisposable
    where TContext : DbContext
{
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> SaveChangesAsync();
}
