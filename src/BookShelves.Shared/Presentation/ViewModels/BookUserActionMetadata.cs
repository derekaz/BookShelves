using System.Text.Json.Serialization;

namespace BookShelves.Shared.Presentation.ViewModels;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BookUserActionToBeReadMetadata), typeDiscriminator: BookUserActionTypes.ToBeRead)]
[JsonDerivedType(typeof(BookUserActionPagesReadMetadata), typeDiscriminator: BookUserActionTypes.PagesRead)]
[JsonDerivedType(typeof(BookUserActionFinishedMetadata), typeDiscriminator: BookUserActionTypes.Finished)]
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
