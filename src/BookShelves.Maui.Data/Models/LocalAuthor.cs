using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.AuthorTable), PrimaryKey(nameof(Id))]
public class LocalAuthor : IAuthor
{
    public LocalAuthor() { }

    public int Id { get; set; }

    public string IdValue => Id.ToString() ?? string.Empty;

    public string? Name { get; set; }

    public string? Biography { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public int? Revision { get; set; }

    public string? UpdateType { get; set; }

    public int? ServerId { get; set; }

    public AuthorItemViewModel ToAuthorItemViewModel()
    {
        return new AuthorItemViewModel()
        {
            Id = Id.ToString() ?? string.Empty,
            Name = Name,
            Biography = Biography,
            LastUpdateTime = LastUpdateTime,
            Revision = Revision,
            //UpdateType = UpdateType,
            //ServerId = ServerId
        };
    }

    public static LocalAuthor FromAuthorItemViewModel(AuthorItemViewModel author)
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

        return new LocalAuthor()
        {
            Id = temp,
            Name = author.Name,
            Biography = author.Biography,
            LastUpdateTime = author.LastUpdateTime,
            Revision = author.Revision,
            //UpdateType = author.UpdateType,
            //ServerId = author.ServerId
        };
    }
}