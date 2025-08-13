internal class Constants
{
    public const string LocalDbFile = "bookshelves.db";
    public const string BookTable = "books";

    public const string CreateBookTableStatement = $"CREATE TABLE IF NOT EXISTS {BookTable} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title VARCHAR(255), Author VARCHAR(255));";

    public const string AllBooksQuery = $"SELECT * FROM {BookTable}";

    // /// <summary>
    // /// The application(client) ID for the native app within Microsoft Entra ID
    // /// </summary>
    //public static string ApplicationId = "7d2baab0-7a8d-4cd6-b898-c0c087b07cc2";
    //public static string ApplicationId = "07654135-9373-4be9-abc4-ad408ada928b";

    //public static string ApplicationId = "1429bc60-21a6-4f87-98a5-27016b33f86a";
    //public static string TenantId = "d23695ff-4843-44ec-8c6f-af2de0c2ccc8";


    // /// <summary>
    // /// The list of scopes to request
    // /// </summary>
    //public static string[] Scopes = new[]
    //{
    //    //"access_via_group_assignments"
    //    "User.Read",
    //    "GroupMember.Read.All"
    //};
}
