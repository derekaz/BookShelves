namespace BookShelves.Maui.Data.Extensions;

public static class SqliteProviderExtension
{
    public static void RegisterSqliteProvider()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
    }
}
