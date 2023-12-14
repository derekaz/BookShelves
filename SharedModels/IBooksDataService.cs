using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public interface IBooksDataService
{
    Task<HttpResponseMessage> CreateBookAsync(Book book);
    Task<HttpResponseMessage> DeleteBookAsync(Book book);
    Task<IEnumerable<Book>> GetBooksAsync();
    Task<HttpResponseMessage> UpdateBookAsync(Book book);
}
