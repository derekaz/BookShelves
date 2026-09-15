using BookShelves.Shared.Components.Pages.MyBooks;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Moq;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Reflection;
using MyBooksIndex = BookShelves.Shared.Components.Pages.MyBooks.Index;

namespace BookShelves.Shared.Tests.Components.Pages;

public sealed class MyBooksIndexLogicTests
{
    [Fact]
    public void FormatActionType_ReturnsReadableLabel_ForKnownTypes()
    {
        var sut = new TestMyBooksIndex();

        Assert.Equal("To Be Read", sut.InvokeFormatActionType(BookUserActionTypes.ToBeRead));
        Assert.Equal("Pages Read", sut.InvokeFormatActionType(BookUserActionTypes.PagesRead));
        Assert.Equal("Finished", sut.InvokeFormatActionType(BookUserActionTypes.Finished));
    }

    [Fact]
    public void FormatActionType_ReturnsRawValue_ForUnknownType()
    {
        var sut = new TestMyBooksIndex();
        Assert.Equal("CustomType", sut.InvokeFormatActionType("CustomType"));
    }

    [Fact]
    public void FormatDate_UsesStartTimeUtc_WhenAvailable()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            StartTimeUtc = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            EndTimeUtc = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero)
        };

        var result = sut.InvokeFormatDate(action);

        Assert.Equal("2024-06-15", result);
    }

    [Fact]
    public void FormatDate_FallsBackToEndTimeUtc_WhenStartTimeIsNull()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            StartTimeUtc = null,
            EndTimeUtc = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero)
        };

        var result = sut.InvokeFormatDate(action);

        Assert.Equal("2024-12-31", result);
    }

    [Fact]
    public void FormatDate_ReturnsEmpty_WhenBothDatesAreNull()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel { StartTimeUtc = null, EndTimeUtc = null };

        Assert.Equal(string.Empty, sut.InvokeFormatDate(action));
    }

    [Fact]
    public void FormatDetails_PagesRead_ShowsPageCount()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            Details = new BookUserActionPagesReadMetadata { PagesRead = 75 }
        };

        var result = sut.InvokeFormatDetails(action);

        Assert.Contains("75 pages", result);
    }

    [Fact]
    public void FormatDetails_Finished_ShowsRating()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            Details = new BookUserActionFinishedMetadata { Rating = 5 }
        };

        var result = sut.InvokeFormatDetails(action);

        Assert.Contains("5", result);
    }

    [Fact]
    public void FormatDetails_ToBeRead_ShowsRemindDate()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            Details = new BookUserActionToBeReadMetadata
            {
                RemindAtUtc = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };

        var result = sut.InvokeFormatDetails(action);

        Assert.Contains("2025-03-01", result);
    }

    [Fact]
    public void FilterFunc_MatchesBookIdAndActionType()
    {
        var sut = new TestMyBooksIndex();
        var action = new BookUserActionViewModel
        {
            BookId = "book-abc",
            ActionType = BookUserActionTypes.ToBeRead
        };

        sut.SetSearchString(string.Empty);
        Assert.True(sut.InvokeFilterFunc(action));

        sut.SetSearchString("abc");
        Assert.True(sut.InvokeFilterFunc(action));

        sut.SetSearchString("ToBeRead");
        Assert.True(sut.InvokeFilterFunc(action));

        sut.SetSearchString("xyz");
        Assert.False(sut.InvokeFilterFunc(action));
    }

    [Fact]
    public void GetBookDisplayName_ReturnsTitle_WhenBookIdIsKnown()
    {
        var sut = new TestMyBooksIndex();
        sut.SetBookTitle("book-1", "Clean Code");

        var value = sut.InvokeGetBookDisplayName("book-1");

        Assert.Equal("Clean Code", value);
    }

    [Fact]
    public void GetBookDisplayName_ReturnsId_WhenBookIdNotMapped()
    {
        var sut = new TestMyBooksIndex();

        var value = sut.InvokeGetBookDisplayName("book-9");

        Assert.Equal("book-9", value);
    }

    [Fact]
    public async Task OnInitializedAsync_LoadsActions_WhenServiceSucceeds()
    {
        var actions = new[]
        {
            new BookUserActionViewModel { BookId = "b1", ActionType = BookUserActionTypes.Finished },
            new BookUserActionViewModel { BookId = "b2", ActionType = BookUserActionTypes.PagesRead }
        };

        var service = new Mock<IBookUserActionsDataService>();
        service.Setup(x => x.GetBookUserActionsAsync(It.IsAny<bool>())).ReturnsAsync(actions);

        var authProvider = new Mock<AuthenticationStateProvider>();
        authProvider
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));

        var sut = new TestMyBooksIndex();
        sut.InitializeDependencies(service.Object, authProvider.Object, Mock.Of<ILogger<MyBooksIndex>>());

        await sut.InvokeOnInitializedAsync();

        service.Verify(x => x.GetBookUserActionsAsync(It.IsAny<bool>()), Times.Once);
        Assert.Equal(2, sut.GetActions().Count());
    }

    [Fact]
    public async Task OnInitializedAsync_SetsMessage_WhenServiceThrows()
    {
        var service = new Mock<IBookUserActionsDataService>();
        service.Setup(x => x.GetBookUserActionsAsync(It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException("fail"));

        var authProvider = new Mock<AuthenticationStateProvider>();
        authProvider
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));

        var sut = new TestMyBooksIndex();
        sut.InitializeDependencies(service.Object, authProvider.Object, Mock.Of<ILogger<MyBooksIndex>>());

        await sut.InvokeOnInitializedAsync();

        Assert.False(string.IsNullOrWhiteSpace(sut.GetMessage()));
    }

    private sealed class TestMyBooksIndex : MyBooksIndex
    {
        public void InitializeDependencies(
            IBookUserActionsDataService service,
            AuthenticationStateProvider authProvider,
            ILogger<MyBooksIndex> logger)
        {
            SetNonPublicProperty(this, "DataService", service);
            SetNonPublicProperty(this, "BooksDataService", Mock.Of<IBooksDataService>());
            SetNonPublicProperty(this, "AuthenticationStateProvider", authProvider);
            SetNonPublicProperty(this, "Logger", logger);
        }

        public void SetSearchString(string value) => searchString = value;

        public IEnumerable<BookUserActionViewModel> GetActions() => actions;

        public string GetMessage() => message;

        public Task InvokeOnInitializedAsync() => OnInitializedAsync();

        public bool InvokeFilterFunc(BookUserActionViewModel action) => FilterFunc(action);

        public string InvokeFormatActionType(string? actionType) => FormatActionType(actionType);

        public string InvokeFormatDate(BookUserActionViewModel action) => FormatDate(action);

        public string InvokeFormatDetails(BookUserActionViewModel action) => FormatDetails(action);

        public string InvokeGetBookDisplayName(string? bookId)
        {
            var method = typeof(MyBooksIndex).GetMethod("GetBookDisplayName", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new InvalidOperationException("GetBookDisplayName method was not found.");
            }

            return (string)(method.Invoke(this, [bookId]) ?? string.Empty);
        }

        public void SetBookTitle(string id, string title)
        {
            var field = typeof(MyBooksIndex).GetField("bookTitlesById", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(this) is not Dictionary<string, string> map)
            {
                throw new InvalidOperationException("bookTitlesById field was not found.");
            }

            map[id] = title;
        }
    }

    private static void SetNonPublicProperty(object instance, string propertyName, object value)
    {
        var type = instance.GetType();
        System.Reflection.PropertyInfo? property = null;
        while (type is not null && property is null)
        {
            property = type.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            type = type.BaseType;
        }
        if (property is null)
            throw new InvalidOperationException($"Property '{propertyName}' not found on type '{instance.GetType().FullName}'.");
        property.SetValue(instance, value);
    }
}
