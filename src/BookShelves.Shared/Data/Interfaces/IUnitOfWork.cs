namespace BookShelves.Shared.Data.Interfaces;

public interface IUnitOfWork<TLocalBook> : IDisposable
    where TLocalBook : class, IBook
{
    IRepository<TLocalBook> LocalBooks { get; }
    // Add other repository properties
    Task<int> CompleteAsync();
}
