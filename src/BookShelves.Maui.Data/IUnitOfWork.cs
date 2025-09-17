using BookShelves.Maui.Data.Models;

namespace BookShelves.Maui.Data;

public interface IUnitOfWork : IDisposable
{
    IRepository<LocalBook> YourEntities { get; }
    // Add other repository properties
    Task<int> CompleteAsync();
}
