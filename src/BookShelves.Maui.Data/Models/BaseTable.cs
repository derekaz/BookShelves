using SQLite;

namespace BookShelves.Maui.Data.Models;

public class BaseTable
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}
