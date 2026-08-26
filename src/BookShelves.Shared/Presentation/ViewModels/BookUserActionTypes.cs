namespace BookShelves.Shared.Presentation.ViewModels;

public static class BookUserActionTypes
{
    public const string ToBeRead = nameof(ToBeRead);
    public const string PagesRead = nameof(PagesRead);
    public const string Finished = nameof(Finished);

    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ToBeRead,
        PagesRead,
        Finished
    };

    public static bool IsSupported(string? actionType)
    {
        return !string.IsNullOrWhiteSpace(actionType) && SupportedTypes.Contains(actionType);
    }

    public static bool RequiresPageCount(string? actionType)
    {
        return string.Equals(actionType, PagesRead, StringComparison.OrdinalIgnoreCase);
    }
}
