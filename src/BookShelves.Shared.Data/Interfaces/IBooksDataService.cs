namespace BookShelves.Shared.Data.Interfaces;

public interface IBooksDataService // where T : IBook
{
    Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false);

    Task<bool> CreateBookAsync(IBook book);

    Task<bool> UpdateBookAsync(IBook book);

    Task<bool> DeleteBookAsync(IBook book, bool softDelete = false);
}
