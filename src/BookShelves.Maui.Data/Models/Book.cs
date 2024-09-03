using BookShelves.Shared.DataInterfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.BookTable)]
public class Book : BaseTable, IBook
{
    public new string? Id { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public Book() { }
}