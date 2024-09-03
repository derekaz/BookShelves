namespace BookShelves.Shared.DataInterfaces;

public interface IBook
{
    string? Id { get; set; }

    string? Title { get; set; }

    string? Author { get; set; }
}
