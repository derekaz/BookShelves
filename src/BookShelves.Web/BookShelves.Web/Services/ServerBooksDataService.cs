using BookShelves.Shared.Data.Interfaces;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;

namespace BookShelves.Web.Services;

internal sealed class ServerBooksDataService  //IHttpClientFactory clientFactory)
    : IBooksDataService
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ServerBooksDataService(//HttpClient? httpClient, 
        IDownstreamApi downstreamApi, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
    {
        //_httpClient = httpClient;
        _downstreamApi = downstreamApi;
        _contextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
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

    public async Task<IEnumerable<IBook>> GetBooksAsync(bool includeSoftDeleted = false)
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
        catch (Exception ex)
        {
            return null;
        }
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