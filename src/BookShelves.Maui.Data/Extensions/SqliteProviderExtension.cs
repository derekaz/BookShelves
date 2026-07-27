namespace BookShelves.Maui.Data.Extensions;

public static class SqliteProviderExtension
{
    /// <summary>
    /// Intentional no-op.
    /// <para>
    /// This library targets <c>net10.0</c>, so platform symbols such as
    /// <c>IOS</c>, <c>ANDROID</c>, etc. are never defined here.
    /// The actual <c>SQLitePCL.raw.SetProvider()</c> call must be made from
    /// the platform-specific app project where those symbols are defined.
    /// See <c>MauiProgram.cs</c> for the real initialisation.
    /// </para>
    /// </summary>
    public static void RegisterSqliteProvider() { }
}
