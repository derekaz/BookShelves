//using System.Net.Http.Json;
//using BookShelves.Shared.DataInterfaces;
//using Microsoft.Extensions.Logging;

//namespace BookShelves.WasmSwa.Data;

//public class BooksDataService : IBooksDataService
//{
//    readonly HttpClient _httpClient;
//    readonly ILogger _logger;

//    public BooksDataService(HttpClient http, ILoggerFactory loggerFactory)
//    {
//        _httpClient = http;
//        _logger = loggerFactory.CreateLogger<BooksDataService>();
//    }

//    public IBook InitializeBookInstance()
//    {
//        return new Book();
//    }

//    public async Task<IEnumerable<IBook>> GetBooksAsync()
//    {
//        try
//        {
//            return await _httpClient.GetFromJsonAsync<Book[]>("/api/books") ?? new Book[] { };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "BooksDataService:GetBooksAsync-Exception");
//            return new Book[] { };
//        }
//    }

//    public async Task<bool> CreateBookAsync(IBook book)
//    {
//        return await Task.FromResult(_httpClient.PostAsJsonAsync("api/books/new", book).Result.IsSuccessStatusCode);
//    }

//    public async Task<bool> UpdateBookAsync(IBook book)
//    {
//        return await Task.FromResult(_httpClient.PostAsJsonAsync("api/books/edit", book).Result.IsSuccessStatusCode);
//    }

//    public async Task<bool> DeleteBookAsync(IBook book)
//    {
//        return await Task.FromResult(_httpClient.DeleteAsync($"/api/book/{book.IdValue}").Result.IsSuccessStatusCode);
//    }
//}