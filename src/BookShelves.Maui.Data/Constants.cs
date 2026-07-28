internal class Constants
{
    public const string LocalDbFile = "bookshelves.db";
    public const string BookTable = "Books";
    public const string AuthorTable = "Authors";
    public const string ConfigurationSettingTable = "ConfigurationSettings";

    //public const string CreateBookTableStatement = $"CREATE TABLE IF NOT EXISTS {BookTable} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(255), Author VARCHAR(255));";

    // public const string AllBooksQuery = $"SELECT * FROM {BookTable}";

    //public const string CreateConfigurationSettingTableStatement = $"CREATE TABLE IF NOT EXISTS {ConfigurationSettingTable} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Key VARCHAR(255), Value VARCHAR(1024)), LastUpdateTime DATETIME;";
}
