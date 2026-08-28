using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Shared.Data;

public class BookUserAction : DatasyncDto
{
    public string BookId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public DateTimeOffset? StartTimeUtc { get; set; }

    public DateTimeOffset? EndTimeUtc { get; set; }

    public BookUserActionMetadata? Details { get; set; }

    public BookUserActionViewModel ToBookUserActionViewModel()
    {
        return new BookUserActionViewModel
        {
            Id = Id,
            BookId = BookId,
            UserId = UserId,
            ActionType = ActionType,
            StartTimeUtc = StartTimeUtc,
            EndTimeUtc = EndTimeUtc,
            Details = Details,
            LastUpdateTime = UpdatedAt,
            Version = Version
        };
    }

    public static BookUserAction FromBookUserActionViewModel(BookUserActionViewModel action)
    {
        return new BookUserAction
        {
            Id = action.Id ?? string.Empty,
            BookId = action.BookId,
            UserId = action.UserId,
            ActionType = action.ActionType,
            StartTimeUtc = action.StartTimeUtc,
            EndTimeUtc = action.EndTimeUtc,
            Details = action.Details,
            UpdatedAt = action.LastUpdateTime,
            Version = action.Version
        };
    }

    public static BookUserAction CreateToBeRead(string bookId, string userId, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, string? notes = null, DateTimeOffset? remindAtUtc = null)
    {
        return new BookUserAction
        {
            BookId = bookId,
            UserId = userId,
            ActionType = BookUserActionTypes.ToBeRead,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            Details = new BookUserActionToBeReadMetadata
            {
                Notes = notes,
                RemindAtUtc = remindAtUtc
            }
        };
    }

    public static BookUserAction CreatePagesRead(string bookId, string userId, int pagesRead, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, string? notes = null)
    {
        return new BookUserAction
        {
            BookId = bookId,
            UserId = userId,
            ActionType = BookUserActionTypes.PagesRead,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            Details = new BookUserActionPagesReadMetadata
            {
                Notes = notes,
                PagesRead = pagesRead
            }
        };
    }

    public static BookUserAction CreateFinished(string bookId, string userId, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, int? rating = null, string? notes = null)
    {
        return new BookUserAction
        {
            BookId = bookId,
            UserId = userId,
            ActionType = BookUserActionTypes.Finished,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            Details = new BookUserActionFinishedMetadata
            {
                Notes = notes,
                Rating = rating
            }
        };
    }
}
