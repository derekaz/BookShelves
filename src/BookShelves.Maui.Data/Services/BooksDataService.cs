using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data.Services;

public class BooksDataService(BookShelvesContext dataContext) : IBooksDataService
{
    readonly BookShelvesContext dataContext = dataContext;

    public IBook InitializeBookInstance()
    {
        return new Book();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync()
    {
        return await dataContext.Books
            .Where(b => b.UpdateType != "D")
            .ToListAsync();
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        var localBook = (Book)book;
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "D";
        localBook.LastUpdateTime = DateTime.UtcNow;
        // dataContext.Books.Remove((Book)book);
        dataContext.Update(localBook); 
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        var localBook = (Book)book;
        localBook.Revision = 0;
        localBook.UpdateType = "C";
        localBook.LastUpdateTime = DateTime.UtcNow;
        await dataContext.Books.AddAsync(localBook);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        var localBook = (Book)book;
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "U";
        localBook.LastUpdateTime = DateTime.UtcNow;
        dataContext.Update(localBook);
        return (await dataContext.SaveChangesAsync()) > 0;
    }
}
