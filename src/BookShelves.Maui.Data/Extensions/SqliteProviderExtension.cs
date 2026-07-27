namespace BookShelves.Maui.Data.Extensions;

public static class SqliteProviderExtension
{
    /// <summary>
    /// Ensures the SQLitePCL provider is initialised for the current platform.
    /// <para>
    /// <c>SQLitePCLRaw.bundle_e_sqlite3</c> registers a module initialiser that calls
    /// <c>raw.SetProvider()</c> automatically when its assembly is first loaded.
    /// Calling <c>SetProvider()</c> a second time throws
    /// <see cref="InvalidOperationException"/>. This method is therefore a no-op when
    /// the bundle has already initialised the provider, which is the normal case.
    /// </para>
    /// </summary>
    public static void RegisterSqliteProvider()
    {
        try
        {
#if WINDOWS || ANDROID
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
#elif IOS || MACCATALYST
            // Use the system-provided SQLite on iOS/macOS instead of the bundled e_sqlite3
            // to avoid native-library link issues when the Data project targets net10.0.
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
#endif
        }
        catch (InvalidOperationException)
        {
            // Provider was already set by the bundle_e_sqlite3 module initialiser – ignore.
        }
    }
}
