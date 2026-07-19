using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services;

internal sealed class ClientAuthorsDataService(HttpClient httpClient) : IAuthorItemDataService
{
    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/authorsdata", author);
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

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        try
        {
            var id = author.Id;
            var response = await httpClient.DeleteAsync($"/authorsdata/{id}");
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

    public async Task<IEnumerable<AuthorItemViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var temp = await httpClient.GetFromJsonAsync<Shared.Data.Author[]>("/authorsdata");
            Console.Write(temp);

            return temp?.Select(a => a.ToAuthorItemViewModel()) ?? throw new IOException("No authors found!");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            // Triggers the interactive login challenge on the client
            exception.Redirect();
            return Array.Empty<AuthorItemViewModel>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        try
        {
            var id = author.Id;
            var response = await httpClient.PutAsJsonAsync($"/authorsdata/{id}", author);
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
