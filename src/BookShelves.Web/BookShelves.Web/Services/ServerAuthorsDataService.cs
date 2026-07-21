using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using CommunityToolkit.Datasync.Client;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.Web.Services;

internal sealed class ServerAuthorsDataService(AuthorsDatasyncClientFactory authorsClientFactory)
        : IAuthorItemDataService
{

    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = new Author
        {
            Name = author.Name,
            Bio = author.Biography,
        };

        var httpClient = authorsClientFactory.CreateClient();
        var tableEndpoint = new Uri("/authors", UriKind.Relative);
        var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

        try
        {
            var result = await authorsClient.AddAsync(newAuthor);

            if (result.IsSuccessful && result.HasValue)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            // Handle the exception as needed
            throw new InvalidOperationException("Error creating author.", ex);
        }

        return false;
    }

    [AuthorizeForScopes(ScopeKeySection = "BooksApi:Scopes")]
    [RequiredScope(RequiredScopesConfigurationKey = "BooksApi:Scopes")]
    public async Task<IEnumerable<AuthorItemViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false)
    {
        try
        {
            return await GetAuthorsDataAsync(includeSoftDeleted);
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<AuthorItemViewModel>> GetAuthorsDataAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var httpClient = authorsClientFactory.CreateClient();
            var tableEndpoint = new Uri("/authors", UriKind.Relative);
            var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

            var authors = await authorsClient.ToListAsync(); //  .Where(item => !item.Deleted).ToListAsync();  //includeSoftDeleted: includeSoftDeleted)

            return authors.Select(a => a.ToAuthorItemViewModel());
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            throw;
        }
    }

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        ArgumentNullException.ThrowIfNull(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(author.Id, nameof(author));

        var newAuthor = new Author
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Biography,
        };

        var httpClient = authorsClientFactory.CreateClient();
        var tableEndpoint = new Uri("/authors", UriKind.Relative);
        var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

        var result = await authorsClient.ReplaceAsync(newAuthor);

        if (result.IsSuccessful && result.HasValue)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        var httpClient = authorsClientFactory.CreateClient();
        var tableEndpoint = new Uri("/authors", UriKind.Relative);
        var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

        // If softDelete is requested, you could implement a different downstream call
        // For now, call the BooksApi delete endpoint which removes the record by id
        ArgumentNullException.ThrowIfNull(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(author.Id, nameof(author));

        var id = author.Id;

        var result = await authorsClient.RemoveAsync(id, new DatasyncServiceOptions());

        if (result.IsSuccessful)
        {
            return true;
        }

        return false;
    }

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }
}
