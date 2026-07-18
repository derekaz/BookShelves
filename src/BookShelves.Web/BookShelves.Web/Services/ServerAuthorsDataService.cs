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
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IServiceProvider _serviceProvider;
    //    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentAndConditionalAccessHandler;

    public ServerAuthorsDataService(//HttpClient? httpClient, 
        IServiceProvider serviceProvider,
        IDownstreamApi downstreamApi,
        IHttpContextAccessor httpContextAccessor
        //      MicrosoftIdentityConsentAndConditionalAccessHandler consentAndConditionalAccessHandler
        )
    {
        _serviceProvider = serviceProvider;
        _downstreamApi = downstreamApi;
        _contextAccessor = httpContextAccessor;
        //_consentAndConditionalAccessHandler = consentAndConditionalAccessHandler;
    }

    public async Task<bool> CreateAuthorAsync(AuthorItemViewModel author)
    {
        var newAuthor = new BookShelves.Web.Shared.Data.Author
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = author.Name,
            Bio = author.Biography,
            // UpdatedAt = author.LastUpdateTime ?? DateTime.UtcNow,
        };

        var httpClient = GetClientFactory().CreateClient();
        var tableEndpoint = new Uri("/authors/new", UriKind.Relative);
        var authorsClient = new DatasyncServiceClient<Author>(tableEndpoint, httpClient);

        var result = await authorsClient.AddAsync(newAuthor);

        if (result.IsSuccessful && result.HasValue)
        {
            return true;
            // return result.Value!;
        }

        return false;

        //HttpContext? context = _contextAccessor.HttpContext;
        //var curUser = context?.User;

        //var newAuthor = new BookShelves.Web.Shared.Data.Author
        //{
        //    Id = string.Empty,
        //    Name = author.Name,
        //    Bio = author.Biography,
        //    UpdatedAt = author.LastUpdateTime ?? DateTime.UtcNow,
        //};

        //var response = await _downstreamApi.CallApiForUserAsync(
        //    "BooksApi",
        //    options =>
        //    {
        //        options.RelativePath = "authors/new";
        //        options.HttpMethod = "post";
        //        options.ContentType = "application/json";
        //    }, curUser, JsonContent.Create(newAuthor));

        //response.EnsureSuccessStatusCode();

        //var createdAuthor = await response.Content.ReadFromJsonAsync<Author>() ??
        //        throw new IOException("No author!");

        //return createdAuthor != null;
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

            var authors = await authorsClient.Where(item => !item.Deleted).ToListAsync();  //includeSoftDeleted: includeSoftDeleted)

            return authors.Select(a => new AuthorItemViewModel
            {
                Id = a.Id,
                Name = a.Name,
                Biography = a.Bio,
                LastUpdateTime = a.UpdatedAt,
            });

            //HttpContext? context = _contextAccessor.HttpContext;
            //var curUser = context?.User;

            //using var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
            //    options =>
            //    {
            //        options.HttpMethod = "get";
            //        options.RelativePath = "authors";
            //    }, curUser);

            //response.EnsureSuccessStatusCode();

            //var rawJson = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"Raw JSON: {rawJson}");

            //var authors = JsonSerializer.Deserialize<Author[]>(rawJson);

            ////var authors = await response.Content.ReadFromJsonAsync<Author[]>() ??
            ////    throw new IOException("No authors!");

            //return authors.Select(a => new AuthorItemViewModel
            //{
            //    Id = a.Id,
            //    Name = a.Name,
            //    Biography = a.Bio,
            //    LastUpdateTime = a.UpdatedAt,
            //});
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

    //private void HandleMsalException(MsalUiRequiredException ex)
    //{
    //    // 1. Pass the current URL as a return parameter so the user redirects back after login
    //    var returnUrl = Uri.EscapeDataString(_navigationManager.Uri);

    //    // 2. Redirect to the login challenge path (e.g., MSAL's default route)
    //    _navigationManager.NavigateTo($"MicrosoftIdentity/Account/SignIn?returnUrl={returnUrl}", forceLoad: true);
    //}

    public async Task<bool> UpdateAuthorAsync(AuthorItemViewModel author)
    {
        HttpContext? context = _contextAccessor.HttpContext;
        var curUser = context?.User;

        var newAuthor = new BookShelves.Web.Shared.Data.Author
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Biography,
            UpdatedAt = author.LastUpdateTime ?? DateTime.UtcNow,
        };

        var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
            options =>
            {
                options.RelativePath = $"/authors/edit/{author.Id}";
                options.HttpMethod = "put";
                options.ContentType = "application/json";
            }, curUser, JsonContent.Create(newAuthor));

        response.EnsureSuccessStatusCode();

        var updatedAuthor = await response.Content.ReadFromJsonAsync<Author>();

        return updatedAuthor != null;
    }

    public async Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false)
    {
        try
        {
            HttpContext? context = _contextAccessor.HttpContext;
            var curUser = context?.User;

            // If softDelete is requested, you could implement a different downstream call
            // For now, call the BooksApi delete endpoint which removes the record by id
            var id = author.Id;

            using var response = await _downstreamApi.CallApiForUserAsync(
                "BooksApi",
                options =>
                {
                    options.RelativePath = $"authors/delete/{id}";
                    options.HttpMethod = "delete";
                }, curUser);

            response.EnsureSuccessStatusCode();

            return response.IsSuccessStatusCode;
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
                //new AuthenticatationDelegatingHandler(),
                //new LoggingHandler(),
                //new ApiKeyRequestHandler("X-API-Key", "my-api-key"),
                //new CustomHttpClientHandler()
            ],
            Timeout = TimeSpan.FromSeconds(120)
        };

        CommunityToolkit.Datasync.Client.Http.HttpClientFactory factory = new(options);
        return factory;
    }
}