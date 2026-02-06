using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

static class MacTokenCacheHelper
{
    // private static IDataProtector? _dataProtector;
    private static readonly ILogger _logger = ApplicationLogger.CreateLogger(nameof(MacTokenCacheHelper));

    public static void EnableSerialization(ITokenCache tokenCache) // , IDataProtector dataProtector)
    {
        // _dataProtector = dataProtector;
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    /// <summary>
    /// Path to the token cache. Note that this could be something different, for instance, for MSIX applications:
    /// private static readonly string CacheFilePath =
    /// $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{AppName}\msalcache.bin";
    /// </summary>
    // public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
    // private static readonly string CacheFilePath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "msalcache.bin"); 
    private static readonly string CacheFilePath = "BookShelves.Maui.msalcache.bin";
    // $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/msalcache.bin";

    private static readonly Lock FileLock = new();

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        // if (_dataProtector == null) throw new NullReferenceException(nameof(_dataProtector));

        lock (FileLock)
        {
            try
            {
                _logger.LogInformation("MacTokenCacheHelper:BeforeAccessNotification-Attempt to read token cache file started-File:{CacheFilePath}", CacheFilePath);
                var temp = SecureStorage.GetAsync(CacheFilePath).Result;
                //args.TokenCache.DeserializeMsalV3(
                //    File.Exists(CacheFilePath)
                //        ? _dataProtector.Unprotect(File.ReadAllBytes(CacheFilePath))
                //        : null
                //);
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
            // if (_dataProtector == null) throw new NullReferenceException(nameof(_dataProtector));

            lock (FileLock)
            {
                // reflect changes in the persistent store
                _logger.LogInformation("MacTokenCacheHelper:AfterAccessNotification-Attempt to write token cache file started");
                try
                {
                    SecureStorage.SetAsync(CacheFilePath, 
                        Convert.ToBase64String(args.TokenCache.SerializeMsalV3())).Wait();
                    //File.WriteAllBytes(
                    //    CacheFilePath,
                    //    _dataProtector.Protect(args.TokenCache.SerializeMsalV3())
                    //);
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