namespace BookShelves.Shared.Presentation.ViewModels;

public class BookUserActionViewModel
{
    public string? Id { get; set; } = string.Empty;

    public string BookId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public DateTimeOffset? StartTimeUtc { get; set; }

    public DateTimeOffset? EndTimeUtc { get; set; }

    public BookUserActionMetadata? Details { get; set; }

    public DateTimeOffset? LastUpdateTime { get; set; } = DateTime.UtcNow;

    public string? Version { get; set; }

    public static BookUserActionViewModel Create(string bookId, string userId, string actionType, DateTimeOffset? startTimeUtc, DateTimeOffset? endTimeUtc, BookUserActionMetadata? details)
    {
        return new BookUserActionViewModel
        {
            BookId = bookId,
            UserId = userId,
            ActionType = actionType,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            Details = details
        };
    }

    public static BookUserActionViewModel CreateToBeRead(string bookId, string userId, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, string? notes = null, DateTimeOffset? remindAtUtc = null)
    {
        return Create(bookId, userId, BookUserActionTypes.ToBeRead, startTimeUtc, endTimeUtc, new BookUserActionToBeReadMetadata
        {
            Notes = notes,
            RemindAtUtc = remindAtUtc
        });
    }

    public static BookUserActionViewModel CreatePagesRead(string bookId, string userId, int pagesRead, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, string? notes = null)
    {
        return Create(bookId, userId, BookUserActionTypes.PagesRead, startTimeUtc, endTimeUtc, new BookUserActionPagesReadMetadata
        {
            Notes = notes,
            PagesRead = pagesRead
        });
    }

    public static BookUserActionViewModel CreateFinished(string bookId, string userId, DateTimeOffset? startTimeUtc = null, DateTimeOffset? endTimeUtc = null, int? rating = null, string? notes = null)
    {
        return Create(bookId, userId, BookUserActionTypes.Finished, startTimeUtc, endTimeUtc, new BookUserActionFinishedMetadata
        {
            Notes = notes,
            Rating = rating
        });
    }
}
