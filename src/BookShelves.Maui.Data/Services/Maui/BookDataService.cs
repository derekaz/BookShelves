using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Models;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services.Maui;

public class BookDataService(IServiceProvider serviceProvider) : IBooksDataService
{
    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        var newBook = Book.FromBookViewModel(book, true);
        newBook.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Book>();
        await repo.AddAsync(newBook);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Book>();
        var localBooks = await repo2.GetAllAsync();

        var repo3 = uow2.GetRepository<Author>();
        var localAuthors = await repo3.GetAllAsync();

        return localBooks.Select(b => b.ToBookViewModel(localAuthors));
    }

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        var localBook = Book.FromBookViewModel(book, false);
        localBook.UpdatedAt = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo = uow.GetRepository<Book>();
        await repo.UpdateAsync(localBook);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book)
    {
        var localBook = Book.FromBookViewModel(book, false);

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork<SyncDbContext>>();
        var repo2 = uow2.GetRepository<Book>();
        await repo2.DeleteAsync(localBook);
        return await uow2.SaveChangesAsync() > 0;
    }
}
