using BookShelves.Shared.Data.Interfaces;
using BookShelves.Web.Shared.Data;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services;

internal sealed class ClientBooksDataService(HttpClient httpClient) : IBooksDataService
{
    public Task<bool> CreateBookAsync(IBook book)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteBookAsync(IBook book, bool softDelete = false)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        var temp = await httpClient.GetFromJsonAsync<Book[]>("/booksdata");

        //if (temp != null)
        //{
        //    foreach (var forecast in temp)
        //    {
        //        forecast.Source = "(via ClientWeatherForecaster) " + forecast.Source;
        //    }
        //}

        var result = temp ?? throw new IOException("No books found!");

        return result ?? throw new IOException("No books found!");
    }

    public Task<bool> UpdateBookAsync(IBook book)
    {
        throw new NotImplementedException();
    }
}
