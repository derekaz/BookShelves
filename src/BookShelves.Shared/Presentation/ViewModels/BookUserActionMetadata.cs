namespace BookShelves.Shared.Presentation.ViewModels;

public class BookUserActionMetadata
{
    public string? Notes { get; set; }
}

public sealed class BookUserActionToBeReadMetadata : BookUserActionMetadata
{
    public DateTimeOffset? RemindAtUtc { get; set; }
}

public sealed class BookUserActionPagesReadMetadata : BookUserActionMetadata
{
    public int PagesRead { get; set; }
}

public sealed class BookUserActionFinishedMetadata : BookUserActionMetadata
{
    public int? Rating { get; set; }
}
