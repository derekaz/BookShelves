using BookShelves.Shared.Components.Pages.MyBooks;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Moq;
using MudBlazor;

namespace BookShelves.Shared.Tests.Components.Pages;

public sealed class BookUserActionDialogTests
{
    // ── AddToBeReadDialog ────────────────────────────────────────────────────

    [Fact]
    public async Task AddToBeReadDialog_Submit_CallsCreateWithToBeReadActionType_AndClosesDialog()
    {
        BookUserActionViewModel? submitted = null;
        var service = new Mock<IBookUserActionsDataService>();
        service
            .Setup(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()))
            .Callback<BookUserActionViewModel>(a => submitted = a)
            .ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult>())).Callback<DialogResult>(r => result = r);

        var sut = new TestAddToBeReadDialog();
        sut.SetParameters("book-1", "user-1");
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), dialog.Object);

        await sut.InvokeSubmitAsync();

        service.Verify(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()), Times.Once);
        Assert.NotNull(submitted);
        Assert.Equal("book-1", submitted!.BookId);
        Assert.Equal("user-1", submitted.UserId);
        Assert.Equal(BookUserActionTypes.ToBeRead, submitted.ActionType);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
    }

    [Fact]
    public async Task AddToBeReadDialog_Submit_ServiceReturnsFalse_DoesNotCloseDialog()
    {
        var service = new Mock<IBookUserActionsDataService>();
        service.Setup(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>())).ReturnsAsync(false);

        var dialog = new Mock<IMudDialogInstance>();

        var sut = new TestAddToBeReadDialog();
        sut.SetParameters("book-1", "user-1");
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), dialog.Object);

        await sut.InvokeSubmitAsync();

        dialog.Verify(x => x.Close(It.IsAny<DialogResult>()), Times.Never);
    }

    [Fact]
    public void AddToBeReadDialog_Cancel_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();

        var sut = new TestAddToBeReadDialog();
        sut.InitializeDependencies(Mock.Of<IBookUserActionsDataService>(), Mock.Of<ISnackbar>(), dialog.Object);

        sut.InvokeCancel();

        dialog.Verify(x => x.Cancel(), Times.Once);
    }

    // ── LogPagesReadDialog ───────────────────────────────────────────────────

    [Fact]
    public async Task LogPagesReadDialog_Submit_WithValidPages_CallsCreateWithPagesReadActionType()
    {
        BookUserActionViewModel? submitted = null;
        var service = new Mock<IBookUserActionsDataService>();
        service
            .Setup(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()))
            .Callback<BookUserActionViewModel>(a => submitted = a)
            .ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult>())).Callback<DialogResult>(r => result = r);

        var sut = new TestLogPagesReadDialog();
        sut.SetParameters("book-2", "user-2");
        sut.SetPagesRead(42);
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), dialog.Object);

        await sut.InvokeSubmitAsync();

        service.Verify(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()), Times.Once);
        Assert.NotNull(submitted);
        Assert.Equal("book-2", submitted!.BookId);
        Assert.Equal("user-2", submitted.UserId);
        Assert.Equal(BookUserActionTypes.PagesRead, submitted.ActionType);
        Assert.IsType<BookUserActionPagesReadMetadata>(submitted.Details);
        Assert.Equal(42, ((BookUserActionPagesReadMetadata)submitted.Details!).PagesRead);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
    }

    [Fact]
    public async Task LogPagesReadDialog_Submit_ZeroPages_DoesNotCallService()
    {
        var service = new Mock<IBookUserActionsDataService>();

        var sut = new TestLogPagesReadDialog();
        sut.SetParameters("book-2", "user-2");
        sut.SetPagesRead(0);
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), new Mock<IMudDialogInstance>().Object);

        await sut.InvokeSubmitAsync();

        service.Verify(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()), Times.Never);
    }

    [Fact]
    public void LogPagesReadDialog_Cancel_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();

        var sut = new TestLogPagesReadDialog();
        sut.InitializeDependencies(Mock.Of<IBookUserActionsDataService>(), Mock.Of<ISnackbar>(), dialog.Object);

        sut.InvokeCancel();

        dialog.Verify(x => x.Cancel(), Times.Once);
    }

    // ── MarkFinishedDialog ───────────────────────────────────────────────────

    [Fact]
    public async Task MarkFinishedDialog_Submit_CallsCreateWithFinishedActionType_AndClosesDialog()
    {
        BookUserActionViewModel? submitted = null;
        var service = new Mock<IBookUserActionsDataService>();
        service
            .Setup(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()))
            .Callback<BookUserActionViewModel>(a => submitted = a)
            .ReturnsAsync(true);

        var dialog = new Mock<IMudDialogInstance>();
        DialogResult? result = null;
        dialog.Setup(x => x.Close(It.IsAny<DialogResult>())).Callback<DialogResult>(r => result = r);

        var sut = new TestMarkFinishedDialog();
        sut.SetParameters("book-3", "user-3");
        sut.SetRating(4);
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), dialog.Object);

        await sut.InvokeSubmitAsync();

        service.Verify(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()), Times.Once);
        Assert.NotNull(submitted);
        Assert.Equal("book-3", submitted!.BookId);
        Assert.Equal("user-3", submitted.UserId);
        Assert.Equal(BookUserActionTypes.Finished, submitted.ActionType);
        Assert.NotNull(submitted.StartTimeUtc);
        Assert.NotNull(submitted.EndTimeUtc);
        Assert.IsType<BookUserActionFinishedMetadata>(submitted.Details);
        Assert.Equal(4, ((BookUserActionFinishedMetadata)submitted.Details!).Rating);
        Assert.NotNull(result);
        Assert.False(result!.Canceled);
    }

    [Fact]
    public async Task MarkFinishedDialog_Submit_NoRating_StillRecordsFinishedAction()
    {
        BookUserActionViewModel? submitted = null;
        var service = new Mock<IBookUserActionsDataService>();
        service
            .Setup(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()))
            .Callback<BookUserActionViewModel>(a => submitted = a)
            .ReturnsAsync(true);

        var sut = new TestMarkFinishedDialog();
        sut.SetParameters("book-3", "user-3");
        sut.InitializeDependencies(service.Object, Mock.Of<ISnackbar>(), new Mock<IMudDialogInstance>().Object);

        await sut.InvokeSubmitAsync();

        service.Verify(x => x.CreateBookUserActionAsync(It.IsAny<BookUserActionViewModel>()), Times.Once);
        Assert.NotNull(submitted);
        Assert.NotNull(submitted!.StartTimeUtc);
        Assert.NotNull(submitted.EndTimeUtc);
        Assert.Null(((BookUserActionFinishedMetadata)submitted.Details!).Rating);
    }

    [Fact]
    public void MarkFinishedDialog_Cancel_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();

        var sut = new TestMarkFinishedDialog();
        sut.InitializeDependencies(Mock.Of<IBookUserActionsDataService>(), Mock.Of<ISnackbar>(), dialog.Object);

        sut.InvokeCancel();

        dialog.Verify(x => x.Cancel(), Times.Once);
    }

    // ── Test harness classes ─────────────────────────────────────────────────

    private sealed class TestAddToBeReadDialog : AddToBeReadDialog
    {
        public void SetParameters(string bookId, string userId)
        {
            BookId = bookId;
            UserId = userId;
        }

        public void InitializeDependencies(IBookUserActionsDataService service, ISnackbar snackbar, IMudDialogInstance dialog)
        {
            SetNonPublicProperty(this, "BookUserActionsDataService", service);
            SetNonPublicProperty(this, "Snackbar", snackbar);
            MudDialog = dialog;
        }

        public Task InvokeSubmitAsync() => SubmitAsync();

        public void InvokeCancel() => Cancel();
    }

    private sealed class TestLogPagesReadDialog : LogPagesReadDialog
    {
        public void SetParameters(string bookId, string userId)
        {
            BookId = bookId;
            UserId = userId;
        }

        public void SetPagesRead(int pages) => pagesRead = pages;

        public void InitializeDependencies(IBookUserActionsDataService service, ISnackbar snackbar, IMudDialogInstance dialog)
        {
            SetNonPublicProperty(this, "BookUserActionsDataService", service);
            SetNonPublicProperty(this, "Snackbar", snackbar);
            MudDialog = dialog;
        }

        public Task InvokeSubmitAsync() => SubmitAsync();

        public void InvokeCancel() => Cancel();
    }

    private sealed class TestMarkFinishedDialog : MarkFinishedDialog
    {
        public void SetParameters(string bookId, string userId)
        {
            BookId = bookId;
            UserId = userId;
        }

        public void SetRating(int? r) => rating = r;

        public void InitializeDependencies(IBookUserActionsDataService service, ISnackbar snackbar, IMudDialogInstance dialog)
        {
            SetNonPublicProperty(this, "BookUserActionsDataService", service);
            SetNonPublicProperty(this, "Snackbar", snackbar);
            MudDialog = dialog;
        }

        public Task InvokeSubmitAsync() => SubmitAsync();

        public void InvokeCancel() => Cancel();
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
