using System.ComponentModel.DataAnnotations.Schema;

public class Book : IBook
{
    public new string Id { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    public Book() { }
}