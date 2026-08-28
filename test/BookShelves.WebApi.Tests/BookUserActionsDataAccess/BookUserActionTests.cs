using System.ComponentModel.DataAnnotations;
using BookUserActionEntity = BookShelves.WebApi.BookUserActionsDataAccess.BookUserAction;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.WebApi.BookUserActionsDataAccess;

namespace BookShelves.WebApi.Tests.BookUserActionsDataAccess;

public sealed class BookUserActionTests
{
    [Fact]
    public void CreatePagesRead_ReturnsPagesReadMetadata()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(3);

        var action = BookUserActionEntity.CreatePagesRead(
            "book-1",
            "user-1",
            start,
            end,
            new BookUserActionPagesReadMetadata { Notes = "progress", PagesRead = 24 });

        Assert.Equal("book-1", action.BookId);
        Assert.Equal("user-1", action.UserId);
        Assert.Equal(BookUserActionTypes.PagesRead, action.ActionType);
        var details = Assert.IsType<BookUserActionPagesReadMetadata>(action.Details);
        Assert.Equal(24, details.PagesRead);
        Assert.Equal("progress", details.Notes);
    }

    [Fact]
    public void Validate_WhenDetailsMissing_ReturnsError()
    {
        var action = CreateValidAction();
        action.Details = null;

        var errors = Validate(action);

        Assert.Contains(errors, error => error.ErrorMessage?.Contains("Details are required", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void Validate_WhenEndBeforeStart_ReturnsError()
    {
        var action = CreateValidAction();
        action.EndTimeUtc = action.StartTimeUtc!.Value.AddMinutes(-1);

        var errors = Validate(action);

        Assert.Contains(errors, error => error.ErrorMessage?.Contains("EndTimeUtc must not be earlier", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void Validate_WhenPagesReadIsNegative_ReturnsError()
    {
        var action = CreateValidAction();
        action.ActionType = BookUserActionTypes.PagesRead;
        action.Details = new BookUserActionPagesReadMetadata { PagesRead = -1 };

        var errors = Validate(action);

        Assert.Contains(errors, error => error.ErrorMessage?.Contains("non-negative", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void Validate_WhenActionTypeUnsupported_ReturnsError()
    {
        var action = CreateValidAction();
        action.ActionType = "Unsupported";

        var errors = Validate(action);

        Assert.Contains(errors, error => error.ErrorMessage?.Contains("Unsupported action type", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void CreateFinished_ReturnsFinishedMetadata()
    {
        var start = DateTimeOffset.UtcNow;
        var end = start.AddMinutes(1);

        var action = BookUserActionEntity.CreateFinished(
            "book-7",
            "user-7",
            start,
            end,
            new BookUserActionFinishedMetadata { Notes = "done", Rating = 5 });

        Assert.Equal(BookUserActionTypes.Finished, action.ActionType);
        var details = Assert.IsType<BookUserActionFinishedMetadata>(action.Details);
        Assert.Equal(5, details.Rating);
        Assert.Equal("done", details.Notes);
    }

    private static BookUserActionEntity CreateValidAction()
    {
        var start = DateTimeOffset.UtcNow;
        return BookUserActionEntity.CreateToBeRead(
            "book-1",
            "user-1",
            start,
            start.AddMinutes(1),
            new BookUserActionToBeReadMetadata { Notes = "note" });
    }

    private static IReadOnlyCollection<ValidationResult> Validate(BookUserActionEntity action)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(action, new ValidationContext(action), results, validateAllProperties: true);
        return results;
    }
}
