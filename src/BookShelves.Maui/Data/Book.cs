using System.ComponentModel.DataAnnotations.Schema;

[Table(Constants.BookTable)]
public class Book : BaseTable, IBook
{
    public new string Id { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    public Book() { }
}