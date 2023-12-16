using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public interface IBooksDataService
{
    IBook InitializeBookInstance();
    Task<bool> CreateBookAsync(IBook book);
    Task<bool> DeleteBookAsync(IBook book);
    Task<IEnumerable<IBook>> GetBooksAsync();
    Task<bool> UpdateBookAsync(IBook book);
}
