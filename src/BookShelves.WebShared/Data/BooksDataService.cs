using System.Net.Http.Json;
using System.Text.Json;
using BookShelves.Shared.DataInterfaces;
using Microsoft.Extensions.Logging;

namespace BookShelves.WebShared.Data;

public class BooksDataService : IBooksDataService
{
    readonly HttpClient _httpClient;
    readonly ILogger _logger;

    public BooksDataService(HttpClient http, ILoggerFactory loggerFactory)
    {
        _httpClient = http;
        _logger = loggerFactory.CreateLogger<BooksDataService>();
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
            _logger.LogError(ex, "BooksDataService:GetBooksAsync-Exception");
            return new Book[] { };
        }
    }

    public async Task<bool> CreateBookAsync(IBook book)
    {
        // var temp = await _httpClient.PostAsJsonAsync("api/books/new", book);
        // return temp.IsSuccessStatusCode;

        var result = await _httpClient.PostAsJsonAsync("api/v2/books/new", book);
        if (!result.IsSuccessStatusCode) return false;

        if (result.Content == null) return true;

        try
        {
            var content = await result.Content.ReadAsStringAsync();
            // var response = JsonConvert.DeserializeObject<ApiResponse<Book>>(content);
            var response = JsonSerializer.Deserialize<ApiResponse<Book>>(content);

            return response?.IsSuccess ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize ApiResponse");
            throw;
        }
    }

    public async Task<bool> UpdateBookAsync(IBook book)
    {
        //var temp = await _httpClient.PostAsJsonAsync("api/books/edit", book);
        //return temp.IsSuccessStatusCode;

        var result = await _httpClient.PostAsJsonAsync("api/v2/books/edit", book);
        if (!result.IsSuccessStatusCode) return false;

        if (result.Content == null) return true;

        try
        {
            var content = await result.Content.ReadAsStringAsync();
            // var response = JsonConvert.DeserializeObject<ApiResponse<Book>>(content);
            var response = JsonSerializer.Deserialize<ApiResponse<Book>>(content);

            return response?.IsSuccess ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to deserialize ApiResponse");
            throw;
        }
    }

    public async Task<bool> DeleteBookAsync(IBook book)
    {
        var temp = await _httpClient.DeleteAsync($"/api/book/{book.IdValue}");
        return temp.IsSuccessStatusCode;
        // return await Task.FromResult(_httpClient.DeleteAsync($"/api/book/{book.IdValue}").Result.IsSuccessStatusCode);
    }
}