namespace BookShelves.Maui.Data.Extensions;

public static class SqliteProviderExtension
{
    public static void RegisterSqliteProvider()
    {
#if WINDOWS || ANDROID
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
#elif IOS || MACCATALYST
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
#endif
    }
}
