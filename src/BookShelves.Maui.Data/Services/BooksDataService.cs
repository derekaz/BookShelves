using BookShelves.Maui.Data.Models;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data.Services;

public class BooksDataService(IServiceProvider serviceProvider) : IBooksDataService
{
    private readonly Expression<Func<LocalBook, bool>> changedBooks = p =>
        p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        var newBook = LocalBook.FromBookViewModel(book);
        newBook.Revision = book.Revision + 1;
        newBook.UpdateType = "C";
        newBook.LastUpdateTime = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        await repo.AddAsync(newBook);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> CreateBookFromSyncAsync(LocalBook book)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        await repo.AddAsync(book);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        var localBook = LocalBook.FromBookViewModel(book);
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "C";
        localBook.LastUpdateTime = DateTime.UtcNow;

        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        await repo.UpdateAsync(localBook);

        var result = await uow.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> UpdateBookFromSyncAsync(LocalBook newBook)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        await repo.AddAsync(newBook);
        return await uow.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
    {
        var localBook = LocalBook.FromBookViewModel(book);

        if (softDelete)
        {
            localBook.Revision = book.Revision + 1;
            localBook.UpdateType = "D";
            localBook.LastUpdateTime = DateTime.UtcNow;

            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repo1 = uow1.GetRepository<LocalBook>();
            await repo1.UpdateAsync(localBook);
            return await uow1.SaveChangesAsync() > 0;
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo2 = uow2.GetRepository<LocalBook>();
        await repo2.DeleteAsync(localBook);
        return await uow2.SaveChangesAsync() > 0;
    }

    //public async Task<IEnumerable<LocalBook>> GetAllEntitiesAsync()
    //{
    //    return await _unitOfWork.LocalBooks.GetAllAsync();
    //}

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted)
        {
            await using var uow1 = serviceProvider.GetRequiredService<IUnitOfWork>();
            var repo1 = uow1.GetRepository<LocalBook>();
            return (await repo1.GetAllAsync()).Select(b => b.ToBookViewModel());
        }

        await using var uow2 = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo2 = uow2.GetRepository<LocalBook>();

        var localBooks = await repo2.FindAsync(b => b.UpdateType != "D");

        return localBooks.Select(b => b.ToBookViewModel());
    }

    public async Task<LocalBook?> GetBookWithServerIdAsync(int serverId)
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        return await repo.FindAsync(b => b.ServerId == serverId).ContinueWith(t => t.Result.FirstOrDefault());
    }

    public async Task<IEnumerable<LocalBook>> GetChangedBooksAsync()
    {
        await using var uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        var repo = uow.GetRepository<LocalBook>();
        return await repo.FindAsync(changedBooks);
    }
}
