using System.Net;
using System.Net.Http.Json;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.WebApi.Tests.Auth;
using BookShelves.WebApi.Tests.TestUtilities;
using BookUserActionEntity = BookShelves.WebApi.BookUserActionsDataAccess.BookUserAction;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class BookUserActionsControllerTests : IClassFixture<BookUserActionsControllerWebApiFactory>
{
    private readonly BookUserActionsControllerWebApiFactory factory;

    public BookUserActionsControllerTests(BookUserActionsControllerWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_BookUserActions_WithoutToken_ReturnsUnauthorized()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/tables/BookUserActions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_BookUserActions_WithNonAdminToken_ReturnsOwnRecordsOnly()
    {
        factory.ResetRepositoryState();

        using var ownClient = factory.CreateClient();
        ownClient.UseTestBearerToken();

        using var adminClient = factory.CreateClient();
        adminClient.UseTestBearerToken();
        adminClient.UseTestRoles("Administrator");

        var ownAction = BookUserActionEntity.CreateFinished(
            "book-own",
            "integration-test-user",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(3),
            new BookUserActionFinishedMetadata { Notes = "mine", Rating = 4 });
        ownAction.Id = "action-own";

        var otherAction = BookUserActionEntity.CreateFinished(
            "book-other",
            "someone-else",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(2),
            new BookUserActionFinishedMetadata { Notes = "theirs", Rating = 5 });
        otherAction.Id = "action-other";

        await CreateActionAsync(ownClient, ownAction);
        await CreateActionAsync(adminClient, otherAction);

        using var ownResponse = await ownClient.GetAsync("/tables/BookUserActions/action-own");
        using var otherResponse = await ownClient.GetAsync("/tables/BookUserActions/action-other");
        var ownStored = await ownResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();

        Assert.Equal(HttpStatusCode.OK, ownResponse.StatusCode);
        Assert.NotNull(ownStored);
        Assert.Equal("integration-test-user", ownStored!.UserId);
        Assert.True(otherResponse.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_BookUserActions_WithAdminToken_ReturnsAllRecords()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();
        client.UseTestRoles("Administrator");

        var ownAction = BookUserActionEntity.CreateToBeRead(
            "book-own",
            "integration-test-user",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(3),
            new BookUserActionToBeReadMetadata { Notes = "mine" });
        ownAction.Id = "action-own";

        var otherAction = BookUserActionEntity.CreatePagesRead(
            "book-other",
            "someone-else",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(2),
            new BookUserActionPagesReadMetadata { Notes = "theirs", PagesRead = 42 });
        otherAction.Id = "action-other";

        await CreateActionAsync(client, ownAction);
        await CreateActionAsync(client, otherAction);

        using var ownResponse = await client.GetAsync("/tables/BookUserActions/action-own");
        using var otherResponse = await client.GetAsync("/tables/BookUserActions/action-other");
        var ownStored = await ownResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();
        var otherStored = await otherResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();

        Assert.Equal(HttpStatusCode.OK, ownResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);
        Assert.NotNull(ownStored);
        Assert.NotNull(otherStored);
        Assert.Equal("integration-test-user", ownStored!.UserId);
        Assert.Equal("someone-else", otherStored!.UserId);
    }

    [Fact]
    public async Task Post_BookUserAction_WithNonAdminToken_NormalizesUserId()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        var action = BookUserActionEntity.CreatePagesRead(
            "book-1",
            "someone-else",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(1),
            new BookUserActionPagesReadMetadata { Notes = "progress", PagesRead = 21 });
        action.Id = "action-1";

        using var response = await client.PostAsJsonAsync("/tables/BookUserActions", action);
        using var readResponse = await client.GetAsync("/tables/BookUserActions/action-1");
        var stored = await readResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.NotNull(stored);
        Assert.Equal("integration-test-user", stored!.UserId);
        Assert.Equal("someone-else", action.UserId);
    }

    [Fact]
    public async Task Post_BookUserAction_WithAdminToken_PreservesTargetUserId()
    {
        factory.ResetRepositoryState();
        using var client = factory.CreateClient();
        client.UseTestBearerToken();
        client.UseTestRoles("Administrator");

        var action = BookUserActionEntity.CreateFinished(
            "book-2",
            "someone-else",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(1),
            new BookUserActionFinishedMetadata { Notes = "done", Rating = 5 });
        action.Id = "action-2";

        using var response = await client.PostAsJsonAsync("/tables/BookUserActions", action);
        using var readResponse = await client.GetAsync("/tables/BookUserActions/action-2");
        var stored = await readResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.NotNull(stored);
        Assert.Equal("someone-else", stored!.UserId);
    }

    [Fact]
    public async Task Put_BookUserAction_WithNonAdminToken_AndMismatchedUserId_IsRejected()
    {
        factory.ResetRepositoryState();

        using var adminClient = factory.CreateClient();
        adminClient.UseTestBearerToken();
        adminClient.UseTestRoles("Administrator");

        using var userClient = factory.CreateClient();
        userClient.UseTestBearerToken();

        var start = DateTimeOffset.UtcNow;
        var seeded = BookUserActionEntity.CreateToBeRead(
            "book-3",
            "someone-else",
            start,
            start.AddMinutes(1),
            new BookUserActionToBeReadMetadata { Notes = "seed" });
        seeded.Id = "action-3";

        await CreateActionAsync(adminClient, seeded);

        var updateAttempt = BookUserActionEntity.CreatePagesRead(
            "book-3",
            "someone-else",
            start,
            start.AddMinutes(2),
            new BookUserActionPagesReadMetadata { Notes = "attempt", PagesRead = 17 });
        updateAttempt.Id = "action-3";

        using var updateResponse = await userClient.PutAsJsonAsync("/tables/BookUserActions/action-3", updateAttempt);
        using var verifyResponse = await adminClient.GetAsync("/tables/BookUserActions/action-3");
        var stored = await verifyResponse.Content.ReadFromJsonAsync<BookUserActionEntity>();

        Assert.True(updateResponse.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden);
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.NotNull(stored);
        Assert.Equal("someone-else", stored!.UserId);
        var details = Assert.IsType<BookUserActionToBeReadMetadata>(stored.Details);
        Assert.Equal("seed", details.Notes);
    }

    private static async Task CreateActionAsync(HttpClient client, BookUserActionEntity action)
    {
        using var response = await client.PostAsJsonAsync("/tables/BookUserActions", action);
        Assert.True(response.IsSuccessStatusCode, $"Expected success creating action but got {response.StatusCode}");
    }
}
