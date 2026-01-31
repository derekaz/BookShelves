namespace BookShelves.Shared.Data.Interfaces;

public interface IBook
{
    string IdValue { get; }

    string? Title { get; set; }

    string? Author { get; set; }

    DateTime? LastUpdateTime { get; set; }

    int? Revision { get; set; }
}
