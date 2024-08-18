using SQLite;

namespace BookShelves.Maui.Data;

public class BaseTable
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}
