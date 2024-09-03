namespace BookShelves.Maui.Helpers;

internal class FileAccessHelper
{
    public static string ApplicationSubPath = Path.Combine("AZMoore", "BookShelves");

    public static string GetLocalFilePath() =>
        GetLocalFilePath(string.Empty, false, string.Empty);

    public static string GetLocalFilePath(string filename) =>
        GetLocalFilePath(string.Empty, false, filename);

    public static string GetLocalFilePath(string subPath, bool ensurePathExists) =>
        GetLocalFilePath(subPath, ensurePathExists, string.Empty);

    public static string GetLocalFilePath(string subPath, bool ensurePathExists, string filename)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, subPath);
        if (ensurePathExists)
        {
            EnsureDirectoryExists(path);
        }
        return Path.Combine(path, filename);
    }

    public static string GetLocalApplicationDataPath(string filename) =>
        GetLocalApplicationDataPath(string.Empty, false, filename);

    public static string GetLocalApplicationDataPath(string subPath, bool ensurePathExists, string filename)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subPath);
        if (ensurePathExists)
        {
            EnsureDirectoryExists(path);
        }
        return Path.Combine(path, filename);
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
