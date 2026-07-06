using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookShelves.Web.Shared.Data;

public class BooksDataService(HttpClient http, ILogger<BooksDataService> logger) : IBooksDataService
{
    private readonly HttpClient _httpClient = http;
    private readonly ILogger _logger = logger;

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BookViewModel[]>("/api/books") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BooksDataService:GetBooksAsync-Exception");
            return [];
        }
    }

    public async Task<bool> CreateBookAsync(BookViewModel book)
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

    public async Task<bool> UpdateBookAsync(BookViewModel book)
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

    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
    {
        var temp = await _httpClient.DeleteAsync($"/api/book/{book.IdValue}");
        return temp.IsSuccessStatusCode;
        // return await Task.FromResult(_httpClient.DeleteAsync($"/api/book/{book.IdValue}").Result.IsSuccessStatusCode);
    }
}