namespace BookShelves.Shared.DataInterfaces;

public interface IBook
{
    string IdValue { get; }

    string? Title { get; set; }

    string? Author { get; set; }
}
