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
        return await dataContext.Books.ToListAsync();
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        dataContext.Books.Remove((Book)book);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        Book b = (Book)book;
        await dataContext.Books.AddAsync(b);
        return (await dataContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        dataContext.Update((Book)book);
        return (await dataContext.SaveChangesAsync()) > 0;
    }
}
