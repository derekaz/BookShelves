using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using System.Linq.Expressions;

namespace BookShelves.Maui.Data;

public class TestBooksService : IBooksDataService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly Expression<Func<LocalBook, bool>> changedBooks = p => 
        p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";

    public TestBooksService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
 
    public async Task<bool> CreateBookAsync(IBook book)
    {
        var newBook = (LocalBook)book;
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

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        var localBook = (LocalBook)book;
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

    public async Task<bool> DeleteBookAsync(IBook book, bool softDelete = false)
    {
        var localBook = (LocalBook)book;

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

    public async Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        if (includeSoftDeleted)
        {
            return await _unitOfWork.LocalBooks.GetAllAsync();
        }

        return await _unitOfWork
            .LocalBooks
            .FindAsync(b => b.UpdateType != "D");
    }

    public async Task<LocalBook?> GetBookWithServerIdAsync(int serverId)
    {
        return (await _unitOfWork
            .LocalBooks
            .FindReadOnlyAsync(b => b.ServerId == serverId))
            .FirstOrDefault();
    }

    public async Task<IEnumerable<LocalBook>> GetChangedBooksAsync()
    {
        return await _unitOfWork
            .LocalBooks
            .FindReadOnlyAsync(changedBooks);
    }
}
