using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

static class MacTokenCacheHelper
{
    private static readonly ILogger _logger = ApplicationLogger.CreateLogger(nameof(MacTokenCacheHelper));

    public static void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);

        if (File.Exists(CacheFilePath))
        {
            _logger.LogInformation("MacTokenCacheHelper:EnableSerialization-Token cache file exists at path:{CacheFilePath}", CacheFilePath);
            File.Delete(CacheFilePath);
            _logger.LogInformation("MacTokenCacheHelper:EnableSerialization-Existing token cache file deleted at path:{CacheFilePath}", CacheFilePath);
        }
    }

    /// <summary>
    /// Represents the full file system path to the local MSAL cache file used for storing authentication data.
    /// </summary>
    /// <remarks>This path is determined using the application's local storage directory and is intended for
    /// use with file-based token caching mechanisms. The file is named "msalcache.bin" and is typically used to persist
    /// authentication tokens securely.</remarks>
    private static readonly string CacheFilePath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "msalcache.bin"); 

    /// <summary>
    /// Represents the storage key used for caching authentication data in the application.
    /// </summary>
    /// <remarks>This key is used to identify the cache file for storing authentication tokens. Changing this
    /// value may result in loss of access to previously cached authentication data.</remarks>
    private static readonly string CacheStorageKey = "BookShelves.Maui.msalcache.bin";

    private static readonly Lock FileLock = new();

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        lock (FileLock)
        {
            try
            {
                _logger.LogInformation("MacTokenCacheHelper:BeforeAccessNotification-Attempt to read token cache file started-File:{CacheFilePath}", CacheStorageKey);
                var temp = SecureStorage.GetAsync(CacheStorageKey).Result;
                args.TokenCache.DeserializeMsalV3(
                    temp != null 
                        ? Convert.FromBase64String(temp) 
                        : null
                    );
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "MacTokenCacheHelper:BeforeAccessNotification-Exception reading token cache");
            }
        }
        _logger.LogInformation("MacTokenCacheHelper:BeforeAccessNotification-Attempt to read token cache file complete");
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged)
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                _logger.LogInformation("MacTokenCacheHelper:AfterAccessNotification-Attempt to write token cache file started");
                try
                {
                    SecureStorage.SetAsync(CacheStorageKey, 
                        Convert.ToBase64String(args.TokenCache.SerializeMsalV3())).Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MacTokenCacheHelper:AfterAccessNotification-Exception writing token cache");
                }
            }
            _logger.LogInformation("MacTokenCacheHelper:AfterAccessNotification-Attempt to write token cache file complete");
        }
        else
        {
            _logger.LogInformation("MacTokenCacheHelper:AfterAccessNotification-Write token cache not required");
        }
    }
}