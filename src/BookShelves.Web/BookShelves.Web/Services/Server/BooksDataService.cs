using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using CommunityToolkit.Datasync.Client;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.Web.Services.Server;

internal sealed class BooksDataService(BooksDatasyncClientFactory booksClientFactory, ILogger<BooksDataService> logger)
        : IBooksDataService
{

    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        var newBook = new Book
        {
            Title = book.Title ?? string.Empty,
            Description = book.Description,
            AuthorId = book.AuthorId,
            PublishedDate = book.PublishDate
        };

        var httpClient = booksClientFactory.CreateClient();
        var tableEndpoint = new Uri("books", UriKind.Relative);
        var booksClient = new DatasyncServiceClient<Book>(tableEndpoint, httpClient);

        try
        {
            var result = await booksClient.AddAsync(newBook);

            if (result.IsSuccessful && result.HasValue)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            // Handle the exception as needed
            throw new InvalidOperationException("Error creating book.", ex);
        }

        return false;
    }

    [AuthorizeForScopes(ScopeKeySection = "BooksApi:Scopes")]
    [RequiredScope(RequiredScopesConfigurationKey = "BooksApi:Scopes")]
    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        try
        {
            return await GetBooksDataAsync(includeSoftDeleted);
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while retrieving books. {ex.Message}  Exception:{ex}");
            throw;
        }
    }

    public async Task<IEnumerable<BookViewModel>> GetBooksDataAsync(bool includeSoftDeleted = false)
    {
        var httpClient = booksClientFactory.CreateClient();
        var tableEndpoint = new Uri("books", UriKind.Relative);
        var booksClient = new DatasyncServiceClient<Book>(tableEndpoint, httpClient);

        try
        {

            var books = await booksClient.ToListAsync(); //  .Where(item => !item.Deleted).ToListAsync();  //includeSoftDeleted: includeSoftDeleted)

            return books.Select(b => b.ToBookViewModel());
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred while retrieving books. {ex.Message}  Exception:{ex};  httpClient.BaseAddress:{httpClient.BaseAddress}");
            throw;
        }
    }

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentException.ThrowIfNullOrWhiteSpace(book.Id, nameof(book));

        var newBook = Book.FromBookViewModel(book);

        var httpClient = booksClientFactory.CreateClient();
        var tableEndpoint = new Uri("books", UriKind.Relative);
        var booksClient = new DatasyncServiceClient<Book>(tableEndpoint, httpClient);

        var result = await booksClient.ReplaceAsync(newBook);

        if (result.IsSuccessful && result.HasValue)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book)

    {
        var httpClient = booksClientFactory.CreateClient();
        var tableEndpoint = new Uri("books", UriKind.Relative);
        var booksClient = new DatasyncServiceClient<Book>(tableEndpoint, httpClient);

        ArgumentNullException.ThrowIfNull(book);
        ArgumentException.ThrowIfNullOrWhiteSpace(book.Id, nameof(book));

        var id = book.Id;

        var result = await booksClient.RemoveAsync(id, new DatasyncServiceOptions());

        if (result.IsSuccessful)
        {
            return true;
        }

        return false;
    }
}







//internal sealed class ServerBooksDataService
//    : IBooksDataService
//{
//    private readonly IDownstreamApi _downstreamApi;
//    private readonly IHttpContextAccessor _contextAccessor;
//    //    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentAndConditionalAccessHandler;

//    public ServerBooksDataService(//HttpClient? httpClient, 
//        IDownstreamApi downstreamApi,
//        IHttpContextAccessor httpContextAccessor
//        //      MicrosoftIdentityConsentAndConditionalAccessHandler consentAndConditionalAccessHandler
//        )
//    {
//        _downstreamApi = downstreamApi;
//        _contextAccessor = httpContextAccessor;
//        //_consentAndConditionalAccessHandler = consentAndConditionalAccessHandler;
//    }

//    public async Task<bool> CreateBookAsync(BookViewModel book)
//    {
//        HttpContext? context = _contextAccessor.HttpContext;
//        var curUser = context?.User;

//        var newBook = new BookShelves.Web.Shared.Data.Book
//        {
//            Id = null,
//            Title = book.Title,
//            Author = book.Author,
//            LastUpdateTime = book.LastUpdateTime,
//        };

//        var response = await _downstreamApi.CallApiForUserAsync(
//            "BooksApi",
//            options =>
//            {
//                options.RelativePath = "books/new";
//                options.HttpMethod = "post";
//                options.ContentType = "application/json";
//            }, curUser, JsonContent.Create(newBook));

//        response.EnsureSuccessStatusCode();

//        var createdBook = await response.Content.ReadFromJsonAsync<Book>() ??
//                throw new IOException("No book!");

//        return createdBook != null;
//    }

//    [AuthorizeForScopes(ScopeKeySection = "BooksApi:Scopes")]
//    [RequiredScope(RequiredScopesConfigurationKey = "BooksApi:Scopes")]
//    public async Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false)
//    {
//        try
//        {
//            return await GetBooksDataAsync(includeSoftDeleted);
//        }
//        catch (MsalUiRequiredException)
//        {
//            throw;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    public async Task<IEnumerable<BookViewModel>> GetBooksDataAsync(bool includeSoftDeleted = false)
//    {
//        try
//        {
//            HttpContext? context = _contextAccessor.HttpContext;
//            var curUser = context?.User;

//            using var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
//                options =>
//                {
//                    options.HttpMethod = "get";
//                    options.RelativePath = "books";
//                }, curUser);

//            response.EnsureSuccessStatusCode();
//            var books = await response.Content.ReadFromJsonAsync<Book[]>() ??
//                throw new IOException("No books!");

//            return books.Select(b => new BookViewModel
//            {
//                Id = b.Id,
//                Title = b.Title,
//                Author = b.Author,
//                LastUpdateTime = b.LastUpdateTime,
//            });
//        }
//        catch (MsalUiRequiredException)
//        {
//            throw;
//        }
//        catch (MicrosoftIdentityWebChallengeUserException)
//        {
//            throw;
//        }
//    }

//    //private void HandleMsalException(MsalUiRequiredException ex)
//    //{
//    //    // 1. Pass the current URL as a return parameter so the user redirects back after login
//    //    var returnUrl = Uri.EscapeDataString(_navigationManager.Uri);

//    //    // 2. Redirect to the login challenge path (e.g., MSAL's default route)
//    //    _navigationManager.NavigateTo($"MicrosoftIdentity/Account/SignIn?returnUrl={returnUrl}", forceLoad: true);
//    //}

//    public async Task<bool> UpdateBookAsync(BookViewModel book)
//    {
//        HttpContext? context = _contextAccessor.HttpContext;
//        var curUser = context?.User;

//        var newBook = new BookShelves.Web.Shared.Data.Book
//        {
//            Id = book.IdValue,
//            Title = book.Title,
//            Author = book.Author,
//            LastUpdateTime = book.LastUpdateTime,
//        };

//        var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
//            options =>
//            {
//                options.RelativePath = $"/books/edit/{book.IdValue}";
//                options.HttpMethod = "put";
//                options.ContentType = "application/json";
//            }, curUser, JsonContent.Create(newBook));

//        response.EnsureSuccessStatusCode();

//        var updatedBook = await response.Content.ReadFromJsonAsync<Book>();

//        return updatedBook != null;
//    }

//    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
//    {
//        try
//        {
//            HttpContext? context = _contextAccessor.HttpContext;
//            var curUser = context?.User;

//            // If softDelete is requested, you could implement a different downstream call
//            // For now, call the BooksApi delete endpoint which removes the record by id
//            var id = book.IdValue;

//            using var response = await _downstreamApi.CallApiForUserAsync(
//                "BooksApi",
//                options =>
//                {
//                    options.RelativePath = $"books/delete/{id}";
//                    options.HttpMethod = "delete";
//                }, curUser);

//            response.EnsureSuccessStatusCode();

//            return response.IsSuccessStatusCode;
//        }
//        catch (MsalUiRequiredException)
//        {
//            throw;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }
//}