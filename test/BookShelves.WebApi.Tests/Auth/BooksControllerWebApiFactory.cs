using System.Reflection;
using BookShelves.WebApi.BooksDataAccess;
using BookShelves.WebApi.DataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class BooksControllerWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CosmosDBConnectionString"] = "AccountEndpoint=https://localhost:8081/;AccountKey=AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSYnKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QA==;",
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "test-tenant",
                ["AzureAd:ClientId"] = "test-client"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddSingleton(CreateRepository());
        });
    }

    private static BookRepository CreateRepository()
    {
        var containerMock = new Mock<Container>(MockBehavior.Loose);
        var iteratorMock = new Mock<FeedIterator<Book>>(MockBehavior.Loose);
        iteratorMock.SetupGet(x => x.HasMoreResults).Returns(false);
        iteratorMock.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("ReadNextAsync should not be called when HasMoreResults is false."));

        containerMock
            .Setup(x => x.GetItemQueryIterator<Book>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns(iteratorMock.Object);
        containerMock
            .Setup(x => x.CreateItemAsync(It.IsAny<Book>(), It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateNoOpItemResponse<Book>());
        containerMock
            .Setup(x => x.UpsertItemAsync(It.IsAny<Book>(), It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateNoOpItemResponse<Book>());
        containerMock
            .Setup(x => x.DeleteItemAsync<Book>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateNoOpItemResponse<Book>());
        containerMock
            .Setup(x => x.ReadItemAsync<Book>(It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateNoOpItemResponse<Book>());

        var client = new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSYnKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QA==;");
        var repository = new BookRepository(new LoggerFactory().CreateLogger<BookRepository>(), client, "test-db", "test-container");

        var containerField = typeof(CosmosRepository<Book>).GetField("container", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate CosmosRepository container field.");

        containerField.SetValue(repository, containerMock.Object);
        return repository;
    }

    private static ItemResponse<T> CreateNoOpItemResponse<T>()
        => (ItemResponse<T>)Activator.CreateInstance(typeof(ItemResponse<T>), nonPublic: true)!;
}
