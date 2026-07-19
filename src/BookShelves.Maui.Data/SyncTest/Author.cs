
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.SyncTest;

public class Author : OfflineClientEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }


    public AuthorItemViewModel ToAuthorItemViewModel()
    {
        return new AuthorItemViewModel()
        {
            Id = Id,
            Name = Name,
            Biography = Bio,
            LastUpdateTime = UpdatedAt,
            // Revision = Revision,
        };
    }

    public static Author FromAuthorItemViewModel(AuthorItemViewModel author)
    {
        //string temp;

        //if (author.Id == "")
        //{
        //    temp = "";
        //}
        //else
        //{
        //    if (!string.TryParse(author.Id, out temp))
        //    {
        //        throw new ArgumentException("Invalid Id value. Cannot convert to int.", nameof(author.Id));
        //    }
        //}

        return new Author()
        {
            Id = author.Id ?? string.Empty, // temp.ToString(),
            Name = author.Name ?? string.Empty,
            Bio = author.Biography,
            UpdatedAt = author.LastUpdateTime,
            //Revision = author.Revision,
            //UpdateType = book.UpdateType,
            //ServerId = book.ServerId
        };
    }

}
