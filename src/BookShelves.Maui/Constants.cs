internal class Constants
{
    public const string LocalDbFile = "bookshelves.db";
    public const string BookTable = "books";

    public const string CreateBookTableStatement = $"CREATE TABLE IF NOT EXISTS {BookTable} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(255), Author VARCHAR(255));";

    public const string AllBooksQuery = $"SELECT * FROM {BookTable}";

}
