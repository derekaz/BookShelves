namespace BookShelves.Shared.Presentation.ViewModels;

public class AuthorItemViewModel
{
    public const string AUTHORITEM_UNIQUEID_RECORD_ID = "**UNIQUEID**";

    public string? Id { get; set; } = string.Empty;

    public string IdValue => Id ?? string.Empty;

    public string? Name { get; set; } = string.Empty;

    public string? Biography { get; set; } = string.Empty;

    public DateTime? LastUpdateTime { get; set; } = DateTime.UtcNow;

    public int? Revision { get; set; } = 0;
}
