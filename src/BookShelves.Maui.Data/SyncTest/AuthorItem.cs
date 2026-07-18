
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.SyncTest;

public class AuthorItem : OfflineClientEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }


    public AuthorItemViewModel ToAuthorItemViewModel()
    {
        return new AuthorItemViewModel()
        {
            Id = Id.ToString() ?? string.Empty,
            Name = Name,
            Biography = Bio,
            LastUpdateTime = UpdatedAt,
            // Revision = Revision,
        };
    }

    public static AuthorItem FromAuthorItemViewModel(AuthorItemViewModel author)
    {
        int temp;

        if (author.Id == "")
        {
            temp = 0;
        }
        else
        {
            if (!int.TryParse(author.Id, out temp))
            {
                throw new ArgumentException("Invalid Id value. Cannot convert to int.", nameof(author.Id));
            }
        }

        return new AuthorItem()
        {
            Id = temp.ToString(),
            Name = author.Name ?? string.Empty,
            Bio = author.Biography,
            UpdatedAt = author.LastUpdateTime,
            //Revision = author.Revision,
            //UpdateType = book.UpdateType,
            //ServerId = book.ServerId
        };
    }

}
