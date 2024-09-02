namespace BookShelves.Maui.Helpers;

internal class FileAccessHelper
{
    public static string ApplicationSubPath = Path.Combine("AZMoore", "BookShelves");

    public static string GetLocalFilePath(string filename) =>
        Path.Combine(FileSystem.AppDataDirectory, filename);

    public static string GetLocalFilePath(string subPath, string filename) =>
        Path.Combine(FileSystem.AppDataDirectory, subPath, filename);

    public static string GetLocalApplicationDataPath(string filename) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);

    public static string GetLocalApplicationDataPath(string subPath, string filename) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subPath, filename);

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
