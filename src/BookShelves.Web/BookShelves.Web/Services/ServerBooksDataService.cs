using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.Web.Services;

internal sealed class ServerBooksDataService
    : IBooksDataService
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    //    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentAndConditionalAccessHandler;

    public ServerBooksDataService(//HttpClient? httpClient, 
        IDownstreamApi downstreamApi,
        IHttpContextAccessor httpContextAccessor
        //      MicrosoftIdentityConsentAndConditionalAccessHandler consentAndConditionalAccessHandler
        )
    {
        _downstreamApi = downstreamApi;
        _contextAccessor = httpContextAccessor;
        //_consentAndConditionalAccessHandler = consentAndConditionalAccessHandler;
    }

    public async Task<bool> CreateBookAsync(BookViewModel book)
    {
        HttpContext? context = _contextAccessor.HttpContext;
        var curUser = context?.User;

        var newBook = new BookShelves.Web.Shared.Data.Book
        {
            Id = null,
            Title = book.Title,
            Author = book.Author,
            LastUpdateTime = book.LastUpdateTime,
        };

        var response = await _downstreamApi.CallApiForUserAsync(
            "BooksApi",
            options =>
            {
                options.RelativePath = "books/new";
                options.HttpMethod = "post";
                options.ContentType = "application/json";
            }, curUser, JsonContent.Create(newBook));

        response.EnsureSuccessStatusCode();

        var createdBook = await response.Content.ReadFromJsonAsync<Book>() ??
                throw new IOException("No book!");

        return createdBook != null;
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
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<BookViewModel>> GetBooksDataAsync(bool includeSoftDeleted = false)
    {
        try
        {
            HttpContext? context = _contextAccessor.HttpContext;
            var curUser = context?.User;

            using var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
                options =>
                {
                    options.HttpMethod = "get";
                    options.RelativePath = "books";
                }, curUser);

            response.EnsureSuccessStatusCode();
            var books = await response.Content.ReadFromJsonAsync<Book[]>() ??
                throw new IOException("No books!");

            return books.Select(b => new BookViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                LastUpdateTime = b.LastUpdateTime,
            });
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

    public async Task<bool> UpdateBookAsync(BookViewModel book)
    {
        HttpContext? context = _contextAccessor.HttpContext;
        var curUser = context?.User;

        var newBook = new BookShelves.Web.Shared.Data.Book
        {
            Id = book.IdValue,
            Title = book.Title,
            Author = book.Author,
            LastUpdateTime = book.LastUpdateTime,
        };

        var response = await _downstreamApi.CallApiForUserAsync("BooksApi",
            options =>
            {
                options.RelativePath = $"/books/edit/{book.IdValue}";
                options.HttpMethod = "put";
                options.ContentType = "application/json";
            }, curUser, JsonContent.Create(newBook));

        response.EnsureSuccessStatusCode();

        var updatedBook = await response.Content.ReadFromJsonAsync<Book>();

        return updatedBook != null;
    }

    public async Task<bool> DeleteBookAsync(BookViewModel book, bool softDelete = false)
    {
        try
        {
            HttpContext? context = _contextAccessor.HttpContext;
            var curUser = context?.User;

            // If softDelete is requested, you could implement a different downstream call
            // For now, call the BooksApi delete endpoint which removes the record by id
            var id = book.IdValue;

            using var response = await _downstreamApi.CallApiForUserAsync(
                "BooksApi",
                options =>
                {
                    options.RelativePath = $"books/delete/{id}";
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
}