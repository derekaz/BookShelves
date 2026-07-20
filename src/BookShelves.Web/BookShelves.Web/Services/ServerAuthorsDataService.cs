using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using CommunityToolkit.Datasync.Client;
using CommunityToolkit.Datasync.Client.Http;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.Web.Services;

internal sealed class ServerAuthorsDataService
    : IAuthorItemDataService
{
    private readonly IServiceProvider _serviceProvider;

    public ServerAuthorsDataService(
        IServiceProvider serviceProvider,
        IDownstreamApi downstreamApi,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = new BookShelves.Web.Shared.Data.Author
        {
            Name = author.Name,
            Bio = author.Biography,
        };

        var httpClient = GetClientFactory().CreateClient();
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
            var httpClient = GetClientFactory().CreateClient();
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
        if (author.Id == null)
        {
            throw new ArgumentNullException(nameof(author.Id), "Author ID cannot be null.");
        }

        var newAuthor = new BookShelves.Web.Shared.Data.Author
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Biography,
        };

        var httpClient = GetClientFactory().CreateClient();
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
        var httpClient = GetClientFactory().CreateClient();
        var tableEndpoint = new Uri("/authors", UriKind.Relative);
        var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

        // If softDelete is requested, you could implement a different downstream call
        // For now, call the BooksApi delete endpoint which removes the record by id
        if (author.Id == null)
        {
            throw new ArgumentNullException(nameof(author.Id), "Author ID cannot be null.");
        }

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

    private HttpClientFactory GetClientFactory()
    {
        var bearerTokenHandler = _serviceProvider.GetRequiredService<BearerTokenHandler>();

        HttpClientOptions options = new()
        {
            Endpoint = new Uri("https://localhost:7135"),
            HttpPipeline = [
                bearerTokenHandler
            ],
            Timeout = TimeSpan.FromSeconds(120)
        };

        CommunityToolkit.Datasync.Client.Http.HttpClientFactory factory = new(options);
        return factory;
    }
}