using BookShelves.Maui.Data.Models;
using BookShelves.Shared.DataInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data;

public class TestBooksService : IBooksDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public TestBooksService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    //public async Task AddNewEntityAsync(LocalBook entity)
    //{
    //    await _unitOfWork.LocalBooks.AddAsync(entity);
    //    await _unitOfWork.CompleteAsync();
    //}

    public async Task<bool> CreateBookAsync(IBook book)
    {
        var newBook = (LocalBook)book;
        await _unitOfWork.LocalBooks.AddAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> CreateBookFromSyncAsync(LocalBook book)
    {
        await _unitOfWork.LocalBooks.AddAsync(book);
        return await _unitOfWork.CompleteAsync() > 0;
    }


    public async Task<bool> DeleteBookAsync(IBook book)
    {
        var newBook = (LocalBook)book;
        await _unitOfWork.LocalBooks.DeleteAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<IEnumerable<LocalBook>> GetAllEntitiesAsync()
    {
        return await _unitOfWork.LocalBooks.GetAllAsync();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync()
    {
        return await _unitOfWork.LocalBooks.GetAllAsync();
    }

    public async Task<LocalBook?> GetBookWithServerIdAsync(int serverId)
    {
        return (await _unitOfWork
            .LocalBooks
            .FindAsync(b => b.ServerId == serverId))
            .FirstOrDefault();

        //return await dataContext
        //    .Books
        //    .AsNoTracking()
        //    .Where(b => b.ServerId == serverId)
        //    .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LocalBook>> GetBooksAsync(Expression<Func<LocalBook, bool>> whereExp)
    {
        return await _unitOfWork.LocalBooks.FindAsync(whereExp);

        //return await dataContext
        //    .Books
        //    .AsNoTracking()
        //    // .Where(b => b.UpdateType != "D")
        //    .Where(whereExp)
        //    .ToListAsync();
    }

    readonly Expression<Func<LocalBook, bool>> changedBooks = p => p.UpdateType == "C" || p.UpdateType == "U" || p.UpdateType == "D";


    public IBook InitializeBookInstance()
    {
        return new LocalBook();
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        var newBook = (LocalBook)book;
        await _unitOfWork.LocalBooks.UpdateAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> UpdateBookFromSyncAsync(LocalBook newBook)
    {
        await _unitOfWork.LocalBooks.UpdateAsync(newBook);
        return await _unitOfWork.CompleteAsync() > 0;

        //dataContext.Update(book);
        //return (await dataContext.SaveChangesAsync()) > 0;
    }

}
