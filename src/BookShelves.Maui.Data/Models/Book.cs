using BookShelves.Shared.DataInterfaces;
using Microsoft.EntityFrameworkCore;

//using SQLite;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.BookTable), PrimaryKey(nameof(Id))]
public class Book : IBook //BaseTable, IBook
{
    //public string? Id { get; set; }
    //[PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string IdValue => Id.ToString() ?? string.Empty;

    public string? Title { get; set; }

    public string? Author { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public int? Revision { get; set; }

    public Book() { }
}