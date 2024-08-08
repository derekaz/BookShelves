namespace BookShelves.Maui.Helpers;

internal class FileAccessHelper
{
    public static string GetLocalFilePath(string filename) =>
        Path.Combine(FileSystem.AppDataDirectory, filename);
}
