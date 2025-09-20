using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data.Services;

public class BooksDataService(BookShelvesDbContext dataContext) : IBooksDataService, IBookFactory
{
    readonly BookShelvesDbContext dataContext = dataContext;

    //public static IBook Create() => new LocalBook();

    public IBook Create()
    {
        return new LocalBook();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        return await dataContext.Books
            .Where(b => b.UpdateType != "D")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<LocalBook>> GetBooksAsync(Expression<Func<LocalBook, bool>> whereExp)
    {
        return await dataContext
            .Books
            .AsNoTracking()
            // .Where(b => b.UpdateType != "D")
            .Where(whereExp)
            .ToListAsync();
    }

    public async Task<LocalBook?> GetBookWithServerIdAsync(int serverId)
    {
        return await dataContext
            .Books
            .AsNoTracking()
            .Where(b => b.ServerId == serverId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteBookAsync(IBook book, bool softDelete = false)
    {
        var localBook = (LocalBook)book;
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "D";
        localBook.LastUpdateTime = DateTime.UtcNow;
        // dataContext.Books.Remove((Book)book);
        dataContext.Update(localBook); 
        // dataContext.Entry<Book>(localBook).State = EntityState.Deleted;
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> CreateBookFromSyncAsync(LocalBook book)
    {
        await dataContext.Books.AddAsync(book);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        var localBook = (LocalBook)book;
        localBook.Revision = 0;
        localBook.UpdateType = "C";
        localBook.LastUpdateTime = DateTime.UtcNow;
        await dataContext.Books.AddAsync(localBook);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateBookFromSyncAsync(LocalBook book)
    {
        dataContext.Update(book);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        var localBook = (LocalBook)book;
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "U";
        localBook.LastUpdateTime = DateTime.UtcNow;
        dataContext.Update(localBook);
        return (await dataContext.SaveChangesAsync()) > 0;
    }
}
