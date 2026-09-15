using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services.Client;

internal sealed class BookUserActionsDataService(HttpClient httpClient) : IBookUserActionsDataService
{
    public async Task<IEnumerable<BookUserActionViewModel>> GetBookUserActionsAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var actions = await httpClient.GetFromJsonAsync<BookUserAction[]>("/bookuseractionsdata");
            return actions?.Select(a => a.ToBookUserActionViewModel()) ?? [];
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return [];
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> CreateBookUserActionAsync(BookUserActionViewModel action)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/bookuseractionsdata", action);
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

    public async Task<bool> UpdateBookUserActionAsync(BookUserActionViewModel action)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/bookuseractionsdata/{action.Id}", action);
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

    public async Task<bool> DeleteBookUserActionAsync(BookUserActionViewModel action)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/bookuseractionsdata/{action.Id}");
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
