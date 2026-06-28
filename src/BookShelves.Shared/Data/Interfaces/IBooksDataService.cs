using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Shared.Data.Interfaces;

public interface IBooksDataService //<T> where T : IBook
{
    Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false);

    Task<bool> CreateBookAsync(BookViewModel book);

    Task<bool> UpdateBookAsync(BookViewModel book);

    Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false);
}
