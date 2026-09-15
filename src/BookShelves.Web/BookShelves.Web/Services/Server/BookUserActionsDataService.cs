using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;
using CommunityToolkit.Datasync.Client;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.Web.Services.Server;

internal sealed class BookUserActionsDataService : IBookUserActionsDataService
{
    private readonly ILogger<BookUserActionsDataService> logger;
    private readonly DatasyncServiceClient<BookUserAction> bookUserActionsClient;

    public BookUserActionsDataService(BookUserActionsDatasyncClientFactory bookUserActionsClientFactory, ILogger<BookUserActionsDataService> logger)
    {
        this.logger = logger;

        var client = bookUserActionsClientFactory.CreateClient();
        bookUserActionsClient = new DatasyncServiceClient<BookUserAction>(new Uri("BookUserActions", UriKind.Relative), client);
    }

    public async Task<bool> CreateBookUserActionAsync(BookUserActionViewModel action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var newAction = BookUserAction.FromBookUserActionViewModel(action);
        newAction.Id = null;
        newAction.UpdatedAt = null;
        newAction.Version = null;

        try
        {
            var result = await bookUserActionsClient.AddAsync(newAction);
            return result.IsSuccessful && result.HasValue;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating book user action.", ex);
        }
    }

    [AuthorizeForScopes(ScopeKeySection = "BooksApi:Scopes")]
    [RequiredScope(RequiredScopesConfigurationKey = "BooksApi:Scopes")]
    public async Task<IEnumerable<BookUserActionViewModel>> GetBookUserActionsAsync(bool includeSoftDeleted = false)
    {
        try
        {
            var actions = await bookUserActionsClient.ToListAsync();
            return actions.Select(a => a.ToBookUserActionViewModel());
        }
        catch (MsalUiRequiredException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving book user actions in {Service}", nameof(BookUserActionsDataService));
            throw;
        }
    }

    public async Task<bool> UpdateBookUserActionAsync(BookUserActionViewModel action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(action.Id, nameof(action));

        var newAction = BookUserAction.FromBookUserActionViewModel(action);
        var result = await bookUserActionsClient.ReplaceAsync(newAction);

        return result.IsSuccessful && result.HasValue;
    }

    public async Task<bool> DeleteBookUserActionAsync(BookUserActionViewModel action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(action.Id, nameof(action));

        var result = await bookUserActionsClient.RemoveAsync(action.Id, new DatasyncServiceOptions());
        return result.IsSuccessful;
    }
}
