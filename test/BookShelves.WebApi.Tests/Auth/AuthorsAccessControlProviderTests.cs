using System.Security.Claims;
using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Http;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class AuthorsAccessControlProviderTests
{
    [Fact]
    public void GetDataView_WhenUserIsAnonymous_DeniesAllItems()
    {
        var provider = CreateProvider(null);

        var predicate = provider.GetDataView().Compile();

        Assert.False(predicate(new AuthorItem { Name = "Any author" }));
    }

    [Fact]
    public void GetDataView_WhenUserIsAuthenticated_AllowsItems()
    {
        var provider = CreateProvider("test-user");

        var predicate = provider.GetDataView().Compile();

        Assert.True(predicate(new AuthorItem { Name = "Any author" }));
    }

    [Theory]
    [InlineData(TableOperation.Query)]
    [InlineData(TableOperation.Read)]
    [InlineData(TableOperation.Create)]
    [InlineData(TableOperation.Update)]
    [InlineData(TableOperation.Delete)]
    public async Task IsAuthorizedAsync_WhenUserIsAuthenticated_AllowsOperations(TableOperation operation)
    {
        var provider = CreateProvider("test-user");

        var allowed = await provider.IsAuthorizedAsync(operation, new AuthorItem { Name = "Any author" });

        Assert.True(allowed);
    }

    private static AuthorsAccessControlProvider CreateProvider(string? userName)
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = userName is null
                ? new DefaultHttpContext()
                : CreateHttpContext(userName)
        };

        return new AuthorsAccessControlProvider(accessor);
    }

    private static HttpContext CreateHttpContext(string userName)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, userName)
        }, "TestAuth"));

        return context;
    }
}
