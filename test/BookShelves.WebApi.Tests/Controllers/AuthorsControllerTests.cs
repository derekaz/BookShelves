using BookShelves.WebApi.AuthorsDataAccess;
using BookShelves.WebApi.Tests.Auth;
using BookShelves.WebApi.Tests.TestUtilities;
using System.Net;
using System.Net.Http.Json;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class AuthorsControllerTests : IClassFixture<AuthorsWebApiFactory>
{
    private readonly AuthorsWebApiFactory factory;

    public AuthorsControllerTests(AuthorsWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_Authors_WithoutToken_ReturnsUnauthorized()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Authors_WithToken_ReturnsSuccess()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Author_WithoutToken_ReturnsUnauthorized()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/tables/Authors", new
        {
            id = "author-no-token-1",
            name = "Token Required"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, factory.GetInvocationCount("CreateAsync"));
    }

    [Fact]
    public async Task Post_Author_WithInvalidName_ReturnsBadRequest()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.PostAsJsonAsync("/tables/Authors", new
        {
            id = "author-invalid-1",
            name = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.GetInvocationCount("CreateAsync"));
    }

    [Fact]
    public async Task Post_GetById_Delete_Author_WithToken_UsesRepositoryPersistenceContract()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var createResponse = await client.PostAsJsonAsync("/tables/Authors", new
        {
            id = "author-contract-1",
            name = "Contract author"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var getResponse = await client.GetAsync("/tables/Authors/author-contract-1");
        var createdAuthor = await getResponse.Content.ReadFromJsonAsync<Author>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(createdAuthor);
        Assert.Equal("author-contract-1", createdAuthor.Id);
        Assert.Equal("Contract author", createdAuthor.Name);

        using var deleteResponse = await client.DeleteAsync("/tables/Authors/author-contract-1");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.True(factory.GetInvocationCount("CreateAsync") >= 1);
        Assert.True(factory.GetInvocationCount("ReadAsync") >= 1);
        Assert.True(factory.GetInvocationCount("DeleteAsync") >= 1);
    }
}
