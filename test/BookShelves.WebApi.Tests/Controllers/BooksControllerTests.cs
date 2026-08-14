using System.Net;
using System.Net.Http.Json;
using BookShelves.WebApi.BooksDataAccess;
using BookShelves.WebApi.Tests.Auth;
using BookShelves.WebApi.Tests.TestUtilities;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class BooksControllerTests : IClassFixture<BooksControllerWebApiFactory>
{
    private readonly BooksControllerWebApiFactory factory;

    public BooksControllerTests(BooksControllerWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_Books_WithoutToken_ReturnsUnauthorized()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/tables/Books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Books_WithToken_ReturnsSuccess()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.GetAsync("/tables/Books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Book_WithoutToken_ReturnsUnauthorized()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/tables/Books", new
        {
            id = "book-no-token-1",
            title = "Token Required"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, factory.GetInvocationCount("CreateAsync"));
    }

    [Fact]
    public async Task Post_Book_WithInvalidTitle_ReturnsBadRequest()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.PostAsJsonAsync("/tables/Books", new
        {
            id = "book-invalid-1",
            title = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.GetInvocationCount("CreateAsync"));
    }

    [Fact]
    public async Task Post_GetById_Delete_Book_WithToken_UsesRepositoryPersistenceContract()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var createResponse = await client.PostAsJsonAsync("/tables/Books", new
        {
            id = "book-contract-1",
            title = "Contract book"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var getResponse = await client.GetAsync("/tables/Books/book-contract-1");
        var createdBook = await getResponse.Content.ReadFromJsonAsync<Book>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(createdBook);
        Assert.Equal("book-contract-1", createdBook.Id);
        Assert.Equal("Contract book", createdBook.Title);

        using var deleteResponse = await client.DeleteAsync("/tables/Books/book-contract-1");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.True(factory.GetInvocationCount("CreateAsync") >= 1);
        Assert.True(factory.GetInvocationCount("ReadAsync") >= 1);
        Assert.True(factory.GetInvocationCount("DeleteAsync") >= 1);
    }
}
