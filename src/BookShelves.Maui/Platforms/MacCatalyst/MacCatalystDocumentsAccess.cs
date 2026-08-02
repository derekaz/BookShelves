using Foundation;
using UIKit;
using UniformTypeIdentifiers;

namespace BookShelves.Maui.Platforms.MacCatalyst;

internal static class MacCatalystDocumentsAccess
{
    private static readonly object SyncRoot = new();
    private static readonly string BookmarkFilePath = Path.Combine(
        FileSystem.AppDataDirectory,
        "BookShelves.MacCatalyst.DocumentsRoot.bookmark");

    private static NSUrl? _resolvedUrl;
    private static string? _resolvedPath;

    internal static string? GetDocumentsRootPath()
    {
        lock (SyncRoot)
        {
            if (!string.IsNullOrWhiteSpace(_resolvedPath))
            {
                return _resolvedPath;
            }

            if (!File.Exists(BookmarkFilePath))
            {
                return null;
            }

            try
            {
                var bookmarkData = NSData.FromFile(BookmarkFilePath);
                if (bookmarkData is null)
                {
                    return null;
                }

                // Explicitly cast the system bitmask integer (1024) to bypass 
                // Mac Catalyst's target restriction warnings on the resolution enum.
                var resolutionOptions = (NSUrlBookmarkResolutionOptions)1024;

                var url = NSUrl.FromBookmarkData(
                    bookmarkData,
                    resolutionOptions,
                    null,
                    out bool stale,
                    out NSError? error);

                if (url is null || error is not null)
                {
                    return null;
                }

                if (!url.StartAccessingSecurityScopedResource())
                {
                    url.Dispose();
                    return null;
                }

                _resolvedUrl?.StopAccessingSecurityScopedResource();
                _resolvedUrl?.Dispose();
                _resolvedUrl = url;
                _resolvedPath = url.Path;

                if (stale)
                {
                    PersistBookmark(url);
                }

                return _resolvedPath;
            }
            catch
            {
                return null;
            }
        }
    }

    internal static async Task<string?> PickAndPersistDocumentsRootAsync()
    {
        var tcs = new TaskCompletionSource<NSUrl?>(TaskCreationOptions.RunContinuationsAsynchronously);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Fixed: Uses the correct UTTypes companion class
            var allowedTypes = new UTType[] { UTTypes.Folder };

            // Fixed: Uses modern constructor targeting init(forOpeningContentTypes:asCopy:)
            var picker = new UIDocumentPickerViewController(allowedTypes, asCopy: false)
            {
                AllowsMultipleSelection = false,
                ModalPresentationStyle = UIModalPresentationStyle.FormSheet
            };

            var pickerDelegate = new FolderPickerDelegate(tcs);
            picker.Delegate = pickerDelegate;

            var presenter = Platform.GetCurrentUIViewController();
            if (presenter is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            presenter.PresentViewController(picker, true, null);
        });

        var selectedUrl = await tcs.Task;
        if (selectedUrl is null)
        {
            return null;
        }

        if (!selectedUrl.StartAccessingSecurityScopedResource())
        {
            selectedUrl.Dispose();
            return null;
        }

        if (!PersistBookmark(selectedUrl))
        {
            selectedUrl.StopAccessingSecurityScopedResource();
            selectedUrl.Dispose();
            return null;
        }

        lock (SyncRoot)
        {
            _resolvedUrl?.StopAccessingSecurityScopedResource();
            _resolvedUrl?.Dispose();
            _resolvedUrl = selectedUrl;
            _resolvedPath = selectedUrl.Path;
        }

        return _resolvedPath;
    }

    internal static void ClearPersistedDocumentsRoot()
    {
        lock (SyncRoot)
        {
            _resolvedUrl?.StopAccessingSecurityScopedResource();
            _resolvedUrl?.Dispose();
            _resolvedUrl = null;
            _resolvedPath = null;

            if (File.Exists(BookmarkFilePath))
            {
                File.Delete(BookmarkFilePath);
            }
        }
    }

    private static bool PersistBookmark(NSUrl url)
    {
        try
        {
            // Fixed: Passed 0 directly as creation options flag to prevent Catalyst obsolescence
            var bookmarkData = url.CreateBookmarkData(
                (NSUrlBookmarkCreationOptions)0,
                null,
                null,
                out NSError? error);

            if (bookmarkData is null || error is not null)
            {
                return false;
            }

            // Fixed: Matching your exact NSData storage preferences safely
            bookmarkData.Save(BookmarkFilePath, true, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class FolderPickerDelegate : UIDocumentPickerDelegate
    {
        private readonly TaskCompletionSource<NSUrl?> _completionSource;

        public FolderPickerDelegate(TaskCompletionSource<NSUrl?> completionSource)
        {
            _completionSource = completionSource;
        }

        // Fixed: Overriding DidPickDocument(..., NSUrl[] urls) instead of singular variant
        // Apple changed the delegate implementation to pass an array even when AllowsMultipleSelection is false.
        public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl[] urls)
        {
            if (urls != null && urls.Length > 0)
            {
                _completionSource.TrySetResult(urls[0]);
            }
            else
            {
                _completionSource.TrySetResult(null);
            }
        }

        public override void WasCancelled(UIDocumentPickerViewController controller)
        {
            _completionSource.TrySetResult(null);
        }
    }
}
