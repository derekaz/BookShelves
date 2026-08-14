using BookShelves.Shared.Components.Pages.Authors;
using BookShelves.Shared.Components.Pages.Books;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Moq;
using MudBlazor;

namespace BookShelves.Shared.Tests.Components.Pages;

public sealed class AuthorBookDetailComponentTests
{
    [Fact]
    public async Task AuthorDetail_SubmitForm_NewRecord_CallsCreateAndClosesWithAddedMessage()
    {
        var authorsService = new Mock<IAuthorsDataService>();
        authorsService.Setup(x => x.CreateAuthorAsync(It.IsAny<AuthorViewModel>())).ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult?>())).Callback<DialogResult?>(r => result = r);

        var sut = new TestAuthorDetail
        {
            ModelObject = new AuthorViewModel
            {
                Id = string.Empty,
                Name = "New Author"
            }
        };
        sut.InitializeDependencies(authorsService.Object, dialog.Object);
        await sut.InvokeOnInitializedAsync();

        await sut.InvokeSubmitForm();

        authorsService.Verify(x => x.CreateAuthorAsync(It.IsAny<AuthorViewModel>()), Times.Once);
        authorsService.Verify(x => x.UpdateAuthorAsync(It.IsAny<AuthorViewModel>()), Times.Never);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
        Assert.Equal("'New Author' added successfully.", result.Data?.ToString());
    }

    [Fact]
    public async Task AuthorDetail_SubmitForm_ExistingRecord_CallsUpdateAndClosesWithUpdatedMessage()
    {
        var authorsService = new Mock<IAuthorsDataService>();
        authorsService.Setup(x => x.UpdateAuthorAsync(It.IsAny<AuthorViewModel>())).ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult?>())).Callback<DialogResult?>(r => result = r);

        var sut = new TestAuthorDetail
        {
            ModelObject = new AuthorViewModel
            {
                Id = "author-1",
                Name = "Existing Author"
            }
        };
        sut.InitializeDependencies(authorsService.Object, dialog.Object);
        await sut.InvokeOnInitializedAsync();

        await sut.InvokeSubmitForm();

        authorsService.Verify(x => x.CreateAuthorAsync(It.IsAny<AuthorViewModel>()), Times.Never);
        authorsService.Verify(x => x.UpdateAuthorAsync(It.IsAny<AuthorViewModel>()), Times.Once);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
        Assert.Equal("'Existing Author' updated successfully.", result.Data?.ToString());
    }

    [Fact]
    public async Task BookDetail_SubmitForm_NewRecord_MapsMatchingAuthorAndCallsCreate()
    {
        var authors = new[]
        {
            new AuthorViewModel { Id = "author-a", Name = "Author A", Biography = "Bio" }
        };

        var authorsService = new Mock<IAuthorsDataService>();
        authorsService.Setup(x => x.GetAuthorsAsync(It.IsAny<bool>())).ReturnsAsync(authors);

        BookViewModel? submittedModel = null;
        var booksService = new Mock<IBooksDataService>();
        booksService
            .Setup(x => x.CreateBookAsync(It.IsAny<BookViewModel>()))
            .Callback<BookViewModel>(m => submittedModel = m)
            .ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult?>())).Callback<DialogResult?>(r => result = r);

        var sut = new TestBookDetail
        {
            ModelObject = new BookViewModel
            {
                Id = string.Empty,
                Title = "New Book",
                Author = new AuthorViewModel { Id = "author-a" }
            }
        };
        sut.InitializeDependencies(booksService.Object, authorsService.Object, dialog.Object);
        await sut.InvokeOnInitializedAsync();

        await sut.InvokeSubmitForm();

        authorsService.Verify(x => x.GetAuthorsAsync(It.IsAny<bool>()), Times.Once);
        booksService.Verify(x => x.CreateBookAsync(It.IsAny<BookViewModel>()), Times.Once);
        booksService.Verify(x => x.UpdateBookAsync(It.IsAny<BookViewModel>()), Times.Never);
        Assert.NotNull(submittedModel);
        Assert.NotNull(submittedModel!.Author);
        Assert.Equal("author-a", submittedModel.Author!.Id);
        Assert.Equal("Author A", submittedModel.Author.Name);
        Assert.NotNull(result);
        Assert.Equal("'New Book' has been added.", result!.Data?.ToString());
    }

    [Fact]
    public async Task BookDetail_SubmitForm_ExistingRecord_CallsUpdateAndClosesWithUpdatedMessage()
    {
        var authorsService = new Mock<IAuthorsDataService>();
        authorsService.Setup(x => x.GetAuthorsAsync(It.IsAny<bool>())).ReturnsAsync(Array.Empty<AuthorViewModel>());

        var booksService = new Mock<IBooksDataService>();
        booksService.Setup(x => x.UpdateBookAsync(It.IsAny<BookViewModel>())).ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult?>())).Callback<DialogResult?>(r => result = r);

        var sut = new TestBookDetail
        {
            ModelObject = new BookViewModel
            {
                Id = "book-1",
                Title = "Existing Book"
            }
        };
        sut.InitializeDependencies(booksService.Object, authorsService.Object, dialog.Object);
        await sut.InvokeOnInitializedAsync();

        await sut.InvokeSubmitForm();

        booksService.Verify(x => x.CreateBookAsync(It.IsAny<BookViewModel>()), Times.Never);
        booksService.Verify(x => x.UpdateBookAsync(It.IsAny<BookViewModel>()), Times.Once);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
        Assert.Equal("'Existing Book' has been updated.", result.Data?.ToString());
    }

    private sealed class TestAuthorDetail : AuthorDetail
    {
        public void InitializeDependencies(IAuthorsDataService dataService, IMudDialogInstance dialog)
        {
            SetNonPublicProperty(this, "AuthorsDataService", dataService);
            MudDialog = dialog;
        }

        public Task InvokeOnInitializedAsync() => OnInitializedAsync();

        public Task InvokeSubmitForm() => SubmitForm();
    }

    private sealed class TestBookDetail : BookDetail
    {
        public void InitializeDependencies(IBooksDataService booksDataService, IAuthorsDataService authorsDataService, IMudDialogInstance dialog)
        {
            SetNonPublicProperty(this, "DataService", booksDataService);
            SetNonPublicProperty(this, "AuthorsDataService", authorsDataService);
            MudDialog = dialog;
        }

        public Task InvokeOnInitializedAsync() => OnInitializedAsync();

        public Task InvokeSubmitForm() => SubmitForm();
    }

    private static void SetNonPublicProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().BaseType?.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' not found on type '{instance.GetType().BaseType?.FullName}'.");
        }

        property.SetValue(instance, value);
    }
}
