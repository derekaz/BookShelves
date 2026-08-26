namespace BookShelves.Shared.Presentation.ViewModels;

public static class BookUserActionDetailsFactory
{
    public static BookUserActionToBeReadMetadata CreateToBeRead(string? notes = null, DateTimeOffset? remindAtUtc = null)
    {
        return new BookUserActionToBeReadMetadata
        {
            Notes = notes,
            RemindAtUtc = remindAtUtc
        };
    }

    public static BookUserActionPagesReadMetadata CreatePagesRead(int pagesRead, string? notes = null)
    {
        return new BookUserActionPagesReadMetadata
        {
            Notes = notes,
            PagesRead = pagesRead
        };
    }

    public static BookUserActionFinishedMetadata CreateFinished(int? rating = null, string? notes = null)
    {
        return new BookUserActionFinishedMetadata
        {
            Notes = notes,
            Rating = rating
        };
    }
}
