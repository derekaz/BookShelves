using BookShelves.Maui.Data.Models;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data.Services;

public class BooksDataService(IUnitOfWork<LocalBook> unitOfWork) : IBooksDataService
{
    private readonly IUnitOfWork<LocalBook> _unitOfWork = unitOfWork;

    private readonly Expression<Func<LocalBook, bool>> changedBooks = p =>
        p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        var newBook = LocalBook.FromBookViewModel(book);
        newBook.Revision = book.Revision + 1;
        newBook.UpdateType = "C";
        newBook.LastUpdateTime = DateTime.UtcNow;
        await _unitOfWork.LocalBooks.AddAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> CreateBookFromSyncAsync(LocalBook book)
    {
        await _unitOfWork.LocalBooks.AddAsync(book);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        var localBook = LocalBook.FromBookViewModel(book);
        localBook.Revision = book.Revision + 1;
        localBook.UpdateType = "C";
        localBook.LastUpdateTime = DateTime.UtcNow;
        await _unitOfWork.LocalBooks.UpdateAsync(localBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> UpdateBookFromSyncAsync(LocalBook newBook)
    {
        await _unitOfWork.LocalBooks.UpdateAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
    {
        var localBook = LocalBook.FromBookViewModel(book);

        if (softDelete)
        {
            localBook.Revision = book.Revision + 1;
            localBook.UpdateType = "D";
            localBook.LastUpdateTime = DateTime.UtcNow;
            await _unitOfWork.LocalBooks.UpdateAsync(localBook);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        await _unitOfWork.LocalBooks.DeleteAsync(localBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    //public async Task<IEnumerable<LocalBook>> GetAllEntitiesAsync()
    //{
    //    return await _unitOfWork.LocalBooks.GetAllAsync();
    //}

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted)
        {
            return (await _unitOfWork.LocalBooks.GetAllAsync()).Select(b => b.ToBookViewModel());
        }

        var localBooks = await _unitOfWork
            .LocalBooks
            .FindAsync(b => b.UpdateType != "D");

        return localBooks.Select(b => b.ToBookViewModel());
    }

    public async Task<LocalBook?> GetBookWithServerIdAsync(int serverId)
    {
        return (await _unitOfWork
            .LocalBooks
            .FindAsync(b => b.ServerId == serverId))
            // .FindReadOnlyAsync(b => b.ServerId == serverId))
            .FirstOrDefault();
    }

    public async Task<IEnumerable<LocalBook>> GetChangedBooksAsync()
    {
        return await _unitOfWork
            .LocalBooks
            .FindAsync(changedBooks);
    }
}
