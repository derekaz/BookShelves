using BookShelves.Shared.Data.Interfaces;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace BookShelves.Web.Services;

internal sealed class ServerBooksDataService  //IHttpClientFactory clientFactory)
    : IBooksDataService
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentAndConditionalAccessHandler;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly NavigationManager _navigationManager;

    public ServerBooksDataService(//HttpClient? httpClient, 
        IDownstreamApi downstreamApi,
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider,
        MicrosoftIdentityConsentAndConditionalAccessHandler consentAndConditionalAccessHandler,
        ITokenAcquisition tokenAcquisition,
        NavigationManager navigationManager)
    {
        //_httpClient = httpClient;
        _downstreamApi = downstreamApi;
        _contextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
        _consentAndConditionalAccessHandler = consentAndConditionalAccessHandler;
        _tokenAcquisition = tokenAcquisition;
        _navigationManager = navigationManager;
    }

    public async Task<bool> CreateBookAsync(IBook book)
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

    // [AuthorizeForScopes(ScopeKeySection = "BooksApi:Scopes")]
    public async Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false)
    {
        try
        {
            return await GetBooksDataAsync(includeSoftDeleted);
        }
        catch (MsalUiRequiredException ex)
        {
            string[] scopes = new[] { "api://[client-id]/access_as_user" };
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            HandleMsalException(ex);
            // _consentAndConditionalAccessHandler?.HandleException(ex);

            try
            {
                return await GetBooksDataAsync(includeSoftDeleted);
            }
            catch (Exception ex2)
            {
                return [];
            }
        }
        catch (Exception ex)
        {
            _consentAndConditionalAccessHandler.HandleException(ex);
            throw;
        }
    }

    public async Task<IEnumerable<IBook>> GetBooksDataAsync(bool includeSoftDeleted = false)
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

            return books;
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            throw ex.MsalUiRequiredException;
        }
    }

    private void HandleMsalException(MsalUiRequiredException ex)
    {
        // 1. Pass the current URL as a return parameter so the user redirects back after login
        var returnUrl = Uri.EscapeDataString(_navigationManager.Uri);

        // 2. Redirect to the login challenge path (e.g., MSAL's default route)
        _navigationManager.NavigateTo($"MicrosoftIdentity/Account/SignIn?returnUrl={returnUrl}", forceLoad: true);
    }

    public async Task<bool> UpdateBookAsync(IBook book)
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
                options.RelativePath = $"/books/{book.IdValue}";
                options.HttpMethod = "put";
                options.ContentType = "application/json";
            }, curUser, JsonContent.Create(newBook));

        response.EnsureSuccessStatusCode();

        var updatedBook = await response.Content.ReadFromJsonAsync<Book[]>() ??
               throw new IOException("No books!");

        return updatedBook != null;
    }

    public Task<bool> DeleteBookAsync(IBook book, bool softDelete = false)
    {
        throw new NotImplementedException();
    }
}