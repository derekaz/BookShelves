using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services.Client;

internal sealed class BooksDataService(HttpClient httpClient) : IBooksDataService
{
    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/booksdata", book);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var tempBooks = await httpClient.GetFromJsonAsync<Book[]>("/booksdata");
            Console.Write(tempBooks);

            var tempAuthors = await httpClient.GetFromJsonAsync<Author[]>("/authorsdata");

            return tempBooks?.Select(b => b.ToBookViewModel(tempAuthors ?? Array.Empty<Author>())) ?? throw new IOException("No books found!");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            // Triggers the interactive login challenge on the client
            exception.Redirect();
            return Array.Empty<BookViewModel>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        try
        {
            var id = book.Id;
            var response = await httpClient.PutAsJsonAsync($"/booksdata/{id}", book);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book)
    {
        try
        {
            var id = book.Id;
            var response = await httpClient.DeleteAsync($"/booksdata/{id}");
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }

}

//internal sealed class ClientBooksDataService(HttpClient httpClient) : IBooksDataService
//{
//    public async Task<bool> CreateBookAsync(BookViewModel book)
//    {
//        try
//        {
//            var response = await httpClient.PostAsJsonAsync("/booksdata", book);
//            response.EnsureSuccessStatusCode();
//            return true;
//        }
//        catch (AccessTokenNotAvailableException exception)
//        {
//            exception.Redirect();
//            return false;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
//    {
//        try
//        {
//            var id = book.IdValue;
//            var response = await httpClient.DeleteAsync($"/booksdata/{id}");
//            response.EnsureSuccessStatusCode();
//            return true;
//        }
//        catch (AccessTokenNotAvailableException exception)
//        {
//            exception.Redirect();
//            return false;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
//    {
//        try
//        {
//            var temp = await httpClient.GetFromJsonAsync<Shared.Data.Book[]>("/booksdata");
//            Console.Write(temp);

//            return temp?.Select(b => b.ToBookViewModel()) ?? throw new IOException("No books found!");
//        }
//        catch (AccessTokenNotAvailableException exception)
//        {
//            // Triggers the interactive login challenge on the client
//            exception.Redirect();
//            return Array.Empty<BookViewModel>();
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    public async Task<bool> UpdateBookAsync(BookViewModel book)
//    {
//        try
//        {
//            var id = book.IdValue;
//            var response = await httpClient.PutAsJsonAsync($"/booksdata/{id}", book);
//            response.EnsureSuccessStatusCode();
//            return true;
//        }
//        catch (AccessTokenNotAvailableException exception)
//        {
//            exception.Redirect();
//            return false;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }
//}
