namespace BookShelves.Shared.DataInterfaces;

public interface IBooksDataService // where T : IBook
{
    IBook InitializeBookInstance();
    Task<IEnumerable<IBook>> GetBooksAsync();
    Task<bool> CreateBookAsync(IBook book);
    Task<bool> UpdateBookAsync(IBook book);
    Task<bool> DeleteBookAsync(IBook book);
}
