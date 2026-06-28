using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.BookTable), PrimaryKey(nameof(Id))]
public class LocalBook : IBook
{
    public LocalBook() { }

    public int Id { get; set; }

    public string IdValue => Id.ToString() ?? string.Empty;

    public string? Title { get; set; }

    public string? Author { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public int? Revision { get; set; }

    public string? UpdateType { get; set; }

    public int? ServerId { get; set; }

    public BookViewModel ToBookViewModel()
    {
        return new BookViewModel()
        {
            Id = Id.ToString() ?? string.Empty,
            Title = Title,
            Author = Author,
            LastUpdateTime = LastUpdateTime,
            Revision = Revision,
            //UpdateType = UpdateType,
            //ServerId = ServerId
        };
    }

    public static LocalBook FromBookViewModel(BookViewModel book)
    {
        int temp;

        if (!int.TryParse(book.Id, out temp))
        {
            throw new ArgumentException("Invalid Id value. Cannot convert to int.", nameof(book.Id));
        }

        return new LocalBook()
        {
            Id = temp,
            Title = book.Title,
            Author = book.Author,
            LastUpdateTime = book.LastUpdateTime,
            Revision = book.Revision,
            //UpdateType = book.UpdateType,
            //ServerId = book.ServerId
        };
    }
}