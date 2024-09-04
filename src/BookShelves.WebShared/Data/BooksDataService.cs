using System.Net.Http.Json;
using BookShelves.Shared.DataInterfaces;

namespace BookShelves.WebShared.Data;

public class BooksDataService : IBooksDataService
{
    readonly HttpClient _httpClient;

    public BooksDataService(HttpClient http)
    {
        _httpClient = http;
    }

    public IBook InitializeBookInstance()
    {
        return new Book();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Book[]>("/api/books") ?? new Book[] { };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new Book[] { };
        }
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        return await Task.FromResult(_httpClient.PostAsJsonAsync("api/books/new", book).Result.IsSuccessStatusCode);
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        return await Task.FromResult(_httpClient.PostAsJsonAsync("api/books/edit", book).Result.IsSuccessStatusCode);
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        return await Task.FromResult(_httpClient.DeleteAsync($"/api/book/{book.IdValue}").Result.IsSuccessStatusCode);
    }
}