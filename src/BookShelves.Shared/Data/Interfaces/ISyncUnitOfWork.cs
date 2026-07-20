using Microsoft.EntityFrameworkCore;
using BookShelves.Shared.Services;

namespace BookShelves.Shared.Data.Interfaces;

public interface ISyncUnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    ISyncProgressService? SyncProgressService { get; set; }
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}
