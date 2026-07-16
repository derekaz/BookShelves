namespace BookShelves.Maui.Data.SyncTest;

public class AuthorItem : OfflineClientEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
}
