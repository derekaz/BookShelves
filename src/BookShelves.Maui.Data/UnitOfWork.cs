using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services;
using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Maui.Data;

// DataAccess/UnitOfWork.cs
public class UnitOfWork(BookShelvesDbContext context) : IUnitOfWork<LocalBook>
{
    private readonly BookShelvesDbContext _context = context;
    private IRepository<LocalBook>? _localBooks;

    public IRepository<LocalBook> LocalBooks => _localBooks ??= new GenericRepository<BookShelvesDbContext, LocalBook>(_context);
    // Initialize other repositories similarly

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
