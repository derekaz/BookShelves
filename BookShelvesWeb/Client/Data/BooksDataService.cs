using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace BlazorApp.Client.Data
{
    public class BooksDataService : IBooksDataService
    {
        readonly HttpClient _httpClient;

        public BooksDataService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            return await _httpClient.GetFromJsonAsync<Book[]>("/api/books") ?? new Book[] { };
        }

        public async Task<HttpResponseMessage> CreateBookAsync(Book book)
        {
            return await _httpClient.PostAsJsonAsync("api/books/new", book);
        }

        public async Task<HttpResponseMessage> UpdateBookAsync(Book book)
        {
            return await _httpClient.PostAsJsonAsync("api/books/edit", book);
        }

        public async Task<HttpResponseMessage> DeleteBookAsync(Book book)
        {
            return await _httpClient.DeleteAsync($"/api/book/{book.Id}");
        }
    }
}
