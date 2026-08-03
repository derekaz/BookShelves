#if MACCATALYST
using BookShelves.Maui.Platforms.MacCatalyst;
#endif

namespace BookShelves.Maui.Helpers;

internal class FileAccessHelper
{
    public static string ApplicationSubPath = Path.Combine("AZMoore", "BookShelves");
    public static string LogsSubPath = "logs";

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

    public static Task<string?> PickMacCatalystDocumentsRootAsync()
    {
#if MACCATALYST
        return MacCatalystDocumentsAccess.PickAndPersistDocumentsRootAsync();
#else
        return Task.FromResult<string?>(null);
#endif
    }

    public static void ClearMacCatalystDocumentsRoot()
    {
#if MACCATALYST
        MacCatalystDocumentsAccess.ClearPersistedDocumentsRoot();
#endif
    }

    public static string GetLogFilePath(string filename)
    {
#if IOS || MACCATALYST
        return GetLocalDocumentsPath(LogsSubPath, true, filename);
#else
        return GetLocalDocumentsPath(Path.Combine(ApplicationSubPath, LogsSubPath), true, filename);
#endif
    }

    public static string GetLocalDocumentsPath(string subPath, bool ensurePathExists, string filename)
    {
        string baseDocumentsPath = string.Empty;
#if ANDROID
        // Maps to: /storage/emulated/0/Android/data/[your.package.name]/files/Documents
        // This directory is automatically visible in modern Android File Managers without needing runtime permissions.
        var androidDocs = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments);
        baseDocumentsPath = androidDocs?.AbsolutePath ?? string.Empty;

#elif IOS
        // Maps to the app's local Documents folder.
        // Read Step 2 below to make this visible in the Apple "Files" App.
        baseDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

#elif MACCATALYST
        // Prefer a user-selected folder with a security-scoped bookmark when one is available.
        // Fall back to the app container Documents folder when the user has not granted access yet.
        baseDocumentsPath = MacCatalystDocumentsAccess.GetDocumentsRootPath()
            ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

#else
        // Windows/Desktop standard visible Documents path
        baseDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

#endif

        var path = Path.Combine(baseDocumentsPath, subPath);
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
