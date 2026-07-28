namespace BookShelves.Shared.Presentation.ViewModels;

public class BookViewModel
{
    public const string AUTHORITEM_UNIQUEID_RECORD_ID = "**UNIQUEID**";

    public string? Id { get; set; } = string.Empty;

    public string IdValue => Id ?? string.Empty;

    public string Title { get; set; } = string.Empty;

    public AuthorViewModel? Author { get; set; }
    // public string? AuthorId { get; set; }

    public string? Description { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTimeOffset? LastUpdateTime { get; set; } = DateTime.UtcNow;
}

//    public const string BOOKS_UNIQUEID_RECORD_ID = "**UNIQUEID**";

//    public string? Id { get; set; } = string.Empty;

//    public string IdValue => Id ?? string.Empty;

//    public string? Title { get; set; } = string.Empty;

//    public string? Author { get; set; } = string.Empty;

//    public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

//    public int? Revision { get; set; } = 0;
//}
