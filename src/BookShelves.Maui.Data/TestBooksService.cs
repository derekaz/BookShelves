using BookShelves.Maui.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Data;

public class TestBooksService
{
    private readonly IUnitOfWork _unitOfWork;

    public TestBooksService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddNewEntityAsync(LocalBook entity)
    {
        await _unitOfWork.LocalBooks.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<LocalBook>> GetAllEntitiesAsync()
    {
        return await _unitOfWork.LocalBooks.GetAllAsync();
    }
}
