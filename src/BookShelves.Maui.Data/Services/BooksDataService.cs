using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.ServiceInterfaces;
using BookShelves.Shared.DataInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data.Services;

public class BooksDataService(IDataService dataService, BookShelvesContext dataContext) : IBooksDataService
{
    readonly IDataService dataService = dataService;
    readonly BookShelvesContext dataContext = dataContext;

    public IBook InitializeBookInstance()
    {
        return new Book();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync()
    {
        return await dataContext.Books.ToListAsync();
        //return await dataService.GetItemsWithQuery<Book>(Constants.AllBooksQuery);
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        dataContext.Books.Remove((Book)book);
        return (await dataContext.SaveChangesAsync()) > 0;
        //return await dataService.ExecuteQuery($"delete from {Constants.BookTable} where Id = {book.Id};");
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        Book b = (Book)book;
        await dataContext.Books.AddAsync(b);
        return (await dataContext.SaveChangesAsync()) > 0;
        //return await dataService.ExecuteQuery($"insert into {Constants.BookTable}(Title, Author) VALUES ('{book.Title}', '{book.Author}');");
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        dataContext.Update((Book)book);
        return (await dataContext.SaveChangesAsync()) > 0;
        //return await dataService.ExecuteQuery($"update {Constants.BookTable} SET Title='{book.Title}', Author='{book.Author}' WHERE Id='{book.Id}';");
    }
}
