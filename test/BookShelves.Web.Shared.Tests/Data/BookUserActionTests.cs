using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;

namespace BookShelves.Web.Shared.Tests.Data;

public sealed class BookUserActionTests
{
    [Fact]
    public void CreatePagesRead_MapsDetailsSubtype()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(5);

        var action = BookUserAction.CreatePagesRead("book-1", "user-1", 123, start, end, "progress note");

        Assert.Equal("book-1", action.BookId);
        Assert.Equal("user-1", action.UserId);
        Assert.Equal(BookUserActionTypes.PagesRead, action.ActionType);
        Assert.Equal(start, action.StartTimeUtc);
        Assert.Equal(end, action.EndTimeUtc);
        var details = Assert.IsType<BookUserActionPagesReadMetadata>(action.Details);
        Assert.Equal(123, details.PagesRead);
        Assert.Equal("progress note", details.Notes);
    }

    [Fact]
    public void ToBookUserActionViewModel_PreservesDetailsSubtype()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(1);
        var action = new BookUserAction
        {
            Id = "action-1",
            BookId = "book-1",
            UserId = "user-1",
            ActionType = BookUserActionTypes.Finished,
            StartTimeUtc = start,
            EndTimeUtc = end,
            Details = new BookUserActionFinishedMetadata
            {
                Notes = "done",
                Rating = 5
            },
            UpdatedAt = start,
            Version = "etag"
        };

        var viewModel = action.ToBookUserActionViewModel();

        Assert.Equal("action-1", viewModel.Id);
        Assert.Equal(BookUserActionTypes.Finished, viewModel.ActionType);
        Assert.Equal(start, viewModel.StartTimeUtc);
        Assert.Equal(end, viewModel.EndTimeUtc);
        var details = Assert.IsType<BookUserActionFinishedMetadata>(viewModel.Details);
        Assert.Equal(5, details.Rating);
        Assert.Equal("done", details.Notes);
        Assert.Equal(start, viewModel.LastUpdateTime);
        Assert.Equal("etag", viewModel.Version);
    }

    [Fact]
    public void FromBookUserActionViewModel_PreservesDetailsSubtype()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(2);
        var viewModel = BookUserActionViewModel.CreateToBeRead(
            "book-9",
            "user-9",
            start,
            end,
            notes: "later",
            remindAtUtc: start.AddDays(1));
        viewModel.Id = "action-9";
        viewModel.LastUpdateTime = end;
        viewModel.Version = "v9";

        var action = BookUserAction.FromBookUserActionViewModel(viewModel);

        Assert.Equal("action-9", action.Id);
        Assert.Equal("book-9", action.BookId);
        Assert.Equal("user-9", action.UserId);
        Assert.Equal(BookUserActionTypes.ToBeRead, action.ActionType);
        Assert.Equal(start, action.StartTimeUtc);
        Assert.Equal(end, action.EndTimeUtc);
        var details = Assert.IsType<BookUserActionToBeReadMetadata>(action.Details);
        Assert.Equal("later", details.Notes);
        Assert.Equal(start.AddDays(1), details.RemindAtUtc);
        Assert.Equal(end, action.UpdatedAt);
        Assert.Equal("v9", action.Version);
    }

    [Fact]
    public void DetailsFactory_CreateFinished_MapsNotesAndRating()
    {
        var details = BookUserActionDetailsFactory.CreateFinished(4, "wrapped up");

        Assert.IsType<BookUserActionFinishedMetadata>(details);
        Assert.Equal(4, details.Rating);
        Assert.Equal("wrapped up", details.Notes);
    }
}
