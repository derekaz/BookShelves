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
            AuthorId = book.Author?.Id,
            PublishDate = book.PublishDate
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
        var authorsClient = new DatasyncServiceClient<Author>(new Uri("authors", UriKind.Relative), httpClient);

        try
        {

            var books = await booksClient.ToListAsync(); //  .Where(item => !item.Deleted).ToListAsync();  //includeSoftDeleted: includeSoftDeleted)
            var authors = await authorsClient.ToListAsync();

            return books.Select(b => b.ToBookViewModel(authors));
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