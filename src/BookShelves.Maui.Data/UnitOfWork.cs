using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data;

// DataAccess/UnitOfWork.cs
public class UnitOfWork(BookShelvesDbContext context) : IUnitOfWork
{
    private readonly BookShelvesDbContext _context = context;
    private IRepository<LocalBook>? _yourEntities;

    public IRepository<LocalBook> YourEntities => _yourEntities ??= new LocalRepository<LocalBook>(_context);
    // Initialize other repositories similarly

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
