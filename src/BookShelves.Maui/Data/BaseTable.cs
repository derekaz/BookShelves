using SQLite;

public class BaseTable
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}
