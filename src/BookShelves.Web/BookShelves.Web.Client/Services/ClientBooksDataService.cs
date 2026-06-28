using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services;

internal sealed class ClientBooksDataService(HttpClient httpClient) : IBooksDataService
{
    public Task<bool> CreateBookAsync(BookViewModel book)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var temp = await httpClient.GetFromJsonAsync<Shared.Data.Book[]>("/booksdata");
            Console.Write(temp);

            return temp?.Select(b => b.ToBookViewModel()) ?? throw new IOException("No books found!");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            // Triggers the interactive login challenge on the client
            exception.Redirect();
            return [];
        }
        catch (Exception ex)
        {
            throw;
        }
        //if (temp != null)
        //{
        //    foreach (var forecast in temp)
        //    {
        //        forecast.Source = "(via ClientWeatherForecaster) " + forecast.Source;
        //    }
        //}

        //var result = temp ?? throw new IOException("No books found!");

        //return result ?? throw new IOException("No books found!");
    }

    public Task<bool> UpdateBookAsync(BookViewModel book)
    {
        throw new NotImplementedException();
    }
}
