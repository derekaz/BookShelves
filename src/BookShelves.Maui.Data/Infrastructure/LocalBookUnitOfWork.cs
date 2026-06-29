namespace BookShelves.Maui.Data.Infrastructure;

// DataAccess/LocalBookUnitOfWork.cs
//public class LocalBookUnitOfWork(BookShelvesDbContext context) : IUnitOfWork<LocalBook>
//{
//    private readonly BookShelvesDbContext _context = context;
//    private IRepository<LocalBook>? _localBooks;

//    public IRepository<LocalBook> LocalBooks => _localBooks ??= new GenericRepository<BookShelvesDbContext, LocalBook>(_context);
//    // Initialize other repositories similarly

//    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

//    public void Dispose() => _context.Dispose();
//}
