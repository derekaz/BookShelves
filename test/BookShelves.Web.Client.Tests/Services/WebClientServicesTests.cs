using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;
using WebAuthor = BookShelves.Web.Shared.Data.Author;
using WebBook = BookShelves.Web.Shared.Data.Book;

namespace BookShelves.Web.Client.Tests.Services;

public sealed class WebClientServicesTests
{
    [Fact]
    public void SyncDataService_DefaultContract_IsNoSyncAndThrowsOnServerSync()
    {
        var sut = CreateInternalInstance<ISyncDataService>("BookShelves.Web.Client.Services.Client.SyncDataService");

        Assert.False(sut.SupportsSync);
        Assert.Throws<NotImplementedException>(() => sut.ServerSyncAsync().GetAwaiter().GetResult());
    }

    [Fact]
    public void AuthenticationUiProvider_ReturnsWebLinkEndpoints()
    {
        var sut = CreateInternalInstance<IAuthenticationUIProvider>("BookShelves.Web.Client.Services.Client.AuthenticationUIProviderService");

        Assert.Equal(AuthenticationUIActionType.Link, sut.LoginActionType);
        Assert.Equal("MicrosoftIdentity/Account/SignIn", sut.GetLoginUrl());
        Assert.Equal(AuthenticationUIActionType.Link, sut.LogoutActionType);
        Assert.Equal("MicrosoftIdentity/Account/SignOut", sut.GetLogoutUrl());
        Assert.True(sut.RequiresNavigation);
        Assert.Equal("Web", sut.PlatformName);
    }

    [Fact]
    public async Task WeatherForecasterService_PrefixesSource_WhenPayloadReturned()
    {
        var payload =
            new[]
            {
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    TemperatureC = 21,
                    Summary = "Warm",
                    Source = "Web API"
                }
            };

        var httpClient = CreateHttpClient(_ => JsonResponse(payload));
        var sut = CreateInternalInstance<IWeatherForecasterService>("BookShelves.Web.Client.Services.Client.WeatherForecasterService", httpClient);

        var result = (await sut.GetWeatherForecastAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("(via ClientWeatherForecaster) Web API", result[0].Source);
    }

    [Fact]
    public void FormFactorService_UsesVersionServiceForVersion()
    {
        var versionService = new StubVersionService("v-test+123");
        var sut = CreateInternalInstance<IFormFactor>("BookShelves.Web.Client.Services.Client.FormFactorService", versionService);

        Assert.Equal("WebAssembly", sut.GetFormFactor());
        Assert.Equal("v-test+123", sut.GetVersion());
    }

    [Fact]
    public void VersionService_ReturnsVersionInfoWithBuild()
    {
        var sut = CreateInternalInstance<IVersionService>("BookShelves.Web.Client.Services.Client.VersionService");

        var version = sut.GetVersion();

        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version.CurrentVersion));
        Assert.Equal("0", version.CurrentBuild);
    }

    [Fact]
    public async Task AuthorsDataService_CrudAndGet_UsesExpectedEndpointsAndMapsResponse()
    {
        var calls = new List<(HttpMethod Method, string Path)>();
        var httpClient = CreateHttpClient(request =>
        {
            calls.Add((request.Method, request.RequestUri?.AbsolutePath ?? string.Empty));

            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/authorsdata")
            {
                var payload = new[]
                {
                    new WebAuthor
                    {
                        Id = "author-1",
                        Name = "Ada",
                        Bio = "Bio",
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                return JsonResponse(payload);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var sut = CreateInternalInstance<IAuthorsDataService>("BookShelves.Web.Client.Services.Client.AuthorsDataService", httpClient);
        var author = new AuthorViewModel { Id = "author-1", Name = "Ada", Biography = "Bio", LastUpdateTime = DateTime.UtcNow };

        Assert.True(await sut.CreateAuthorAsync(author));
        var fetched = (await sut.GetAuthorsAsync()).ToList();
        Assert.True(await sut.UpdateAuthorAsync(author));
        Assert.True(await sut.DeleteAuthorAsync(author));

        Assert.Single(fetched);
        Assert.Equal("author-1", fetched[0].Id);
        Assert.Equal("Ada", fetched[0].Name);
        Assert.Equal("Bio", fetched[0].Biography);

        Assert.Equal(
            [
                (HttpMethod.Post, "/authorsdata"),
                (HttpMethod.Get, "/authorsdata"),
                (HttpMethod.Put, "/authorsdata/author-1"),
                (HttpMethod.Delete, "/authorsdata/author-1")
            ],
            calls);
    }

    [Fact]
    public async Task BooksDataService_CrudAndGet_UsesExpectedEndpointsAndMapsAuthor()
    {
        var calls = new List<(HttpMethod Method, string Path)>();
        var httpClient = CreateHttpClient(request =>
        {
            calls.Add((request.Method, request.RequestUri?.AbsolutePath ?? string.Empty));

            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/booksdata")
            {
                var booksPayload = new[]
                {
                    new WebBook
                    {
                        Id = "book-1",
                        Title = "Book A",
                        AuthorId = "author-1",
                        Description = "Desc",
                        PublishDate = new DateTime(2020, 1, 1),
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                return JsonResponse(booksPayload);
            }

            if (request.Method == HttpMethod.Get && request.RequestUri?.AbsolutePath == "/authorsdata")
            {
                var authorsPayload = new[]
                {
                    new WebAuthor
                    {
                        Id = "author-1",
                        Name = "Ada",
                        Bio = "Bio",
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                return JsonResponse(authorsPayload);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var sut = CreateInternalInstance<IBooksDataService>("BookShelves.Web.Client.Services.Client.BooksDataService", httpClient);
        var book = new BookViewModel
        {
            Id = "book-1",
            Title = "Book A",
            Author = new AuthorViewModel { Id = "author-1", Name = "Ada" },
            Description = "Desc",
            PublishDate = new DateTime(2020, 1, 1),
            LastUpdateTime = DateTime.UtcNow
        };

        Assert.True(await sut.CreateBookAsync(book));
        var fetched = (await sut.GetBooksAsync()).ToList();
        Assert.True(await sut.UpdateBookAsync(book));
        Assert.True(await sut.DeleteBookAsync(book));

        Assert.Single(fetched);
        Assert.Equal("book-1", fetched[0].Id);
        Assert.Equal("Book A", fetched[0].Title);
        Assert.NotNull(fetched[0].Author);
        Assert.Equal("author-1", fetched[0].Author?.Id);

        Assert.Equal(
            [
                (HttpMethod.Post, "/booksdata"),
                (HttpMethod.Get, "/booksdata"),
                (HttpMethod.Get, "/authorsdata"),
                (HttpMethod.Put, "/booksdata/book-1"),
                (HttpMethod.Delete, "/booksdata/book-1")
            ],
            calls);
    }

    private static T CreateInternalInstance<T>(string fullTypeName, params object[] args)
    {
        var assembly = Assembly.Load("BookShelves.Web.Client");
        var type = assembly.GetType(fullTypeName) ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found.");

        return (T)(Activator.CreateInstance(type, args) ?? throw new InvalidOperationException($"Could not create '{fullTypeName}'."));
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHandler(responder);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://bookshelves.test/")
        };
    }

    private static HttpResponseMessage JsonResponse<T>(T payload)
        => new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload)
        };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class StubVersionService(string version) : IVersionService
    {
        public VersionInfo GetVersion() => new() { CurrentVersion = version, CurrentBuild = "0" };
    }
}
