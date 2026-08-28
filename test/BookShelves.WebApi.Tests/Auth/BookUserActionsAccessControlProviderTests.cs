using System.Security.Claims;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.WebApi.BookUserActionsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Http;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class BookUserActionsAccessControlProviderTests
{
    [Fact]
    public void GetDataView_WhenUserIsAnonymous_DeniesAllItems()
    {
        var provider = CreateProvider(null);

        var predicate = provider.GetDataView().Compile();

        Assert.False(predicate(CreateAction("owner-a")));
    }

    [Fact]
    public void GetDataView_WhenUserIsAuthenticated_FiltersToOwnRecords()
    {
        var provider = CreateProvider("user-a");

        var predicate = provider.GetDataView().Compile();

        Assert.True(predicate(CreateAction("user-a")));
        Assert.False(predicate(CreateAction("user-b")));
    }

    [Fact]
    public void GetDataView_WhenUserIsAdmin_AllowsAllRecords()
    {
        var provider = CreateProvider("admin-user", admin: true);

        var predicate = provider.GetDataView().Compile();

        Assert.True(predicate(CreateAction("user-a")));
        Assert.True(predicate(CreateAction("user-b")));
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenNonAdminQueriesWithNullEntity_Allows()
    {
        var provider = CreateProvider("user-a");

        var allowed = await provider.IsAuthorizedAsync(TableOperation.Query, null);

        Assert.True(allowed);
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenNonAdminCreatesWithNullEntity_Allows()
    {
        var provider = CreateProvider("user-a");

        var allowed = await provider.IsAuthorizedAsync(TableOperation.Create, null);

        Assert.True(allowed);
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenNonAdminCreates_RecordIsNormalizedToCurrentUser()
    {
        var provider = CreateProvider("user-a");
        var action = CreateAction("user-b");

        var allowed = await provider.IsAuthorizedAsync(TableOperation.Create, action);

        Assert.True(allowed);
        Assert.Equal("user-a", action.UserId);
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenNonAdminUpdatesOtherUsersRecord_Denies()
    {
        var provider = CreateProvider("user-a");

        var allowed = await provider.IsAuthorizedAsync(TableOperation.Update, CreateAction("user-b"));

        Assert.False(allowed);
    }

    [Fact]
    public async Task IsAuthorizedAsync_WhenAdminUpdatesOtherUsersRecord_Allows()
    {
        var provider = CreateProvider("admin-user", admin: true);

        var allowed = await provider.IsAuthorizedAsync(TableOperation.Update, CreateAction("user-b"));

        Assert.True(allowed);
    }

    private static BookUserActionsAccessControlProvider CreateProvider(string? userName, bool admin = false)
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = userName is null
                ? new DefaultHttpContext()
                : CreateHttpContext(userName, admin)
        };

        return new BookUserActionsAccessControlProvider(accessor);
    }

    private static HttpContext CreateHttpContext(string userName, bool admin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userName),
            new(ClaimTypes.Name, userName)
        };

        if (admin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return context;
    }

    private static BookUserAction CreateAction(string userId)
    {
        return BookUserAction.CreateToBeRead(
            "book-1",
            userId,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(5),
            new BookUserActionToBeReadMetadata { Notes = "note" });
    }
}
