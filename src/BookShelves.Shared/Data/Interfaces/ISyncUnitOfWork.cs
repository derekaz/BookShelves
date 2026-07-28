using Microsoft.EntityFrameworkCore;

namespace BookShelves.Shared.Data.Interfaces;

public interface ISyncUnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    Task SynchronizeAsync(CancellationToken cancellationToken = default);
}
