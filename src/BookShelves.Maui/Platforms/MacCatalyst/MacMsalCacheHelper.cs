using Microsoft.AspNetCore.DataProtection;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

static class MacTokenCacheHelper
{
    static IDataProtector? _dataProtector;

    public static void EnableSerialization(ITokenCache tokenCache, IDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
    }

    /// <summary>
    /// Path to the token cache. Note that this could be something different, for instance, for MSIX applications:
    /// private static readonly string CacheFilePath =
    /// $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{AppName}\msalcache.bin";
    /// </summary>
    // public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
    private static readonly string CacheFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/msalcache.bin";

    private static readonly object FileLock = new object();

    private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        if (_dataProtector == null) throw new NullReferenceException(nameof(_dataProtector));

        lock (FileLock)
        {
            try
            {
                Console.WriteLine("MacTokenCacheHelper:BeforeAccessNotification-Attempt to read token cache file started-File:{0}", CacheFilePath);
                args.TokenCache.DeserializeMsalV3(
                    File.Exists(CacheFilePath)
                        ? _dataProtector.Unprotect(File.ReadAllBytes(CacheFilePath))
                        : null
                );
                //args.TokenCache.DeserializeMsalV3(
                //    File.Exists(CacheFilePath)
                //        ? ProtectedData.Unprotect(
                //            File.ReadAllBytes(CacheFilePath),
                //            null,
                //            DataProtectionScope.CurrentUser
                //          )
                //        : null
                //);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("MacTokenCacheHelper:BeforeAccessNotification-Exception reading token cache - {0}", ex);
            }
        }
        Console.WriteLine("MacTokenCacheHelper:BeforeAccessNotification-Attempt to read token cache file complete");
    }

    private static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        // if the access operation resulted in a cache update
        if (args.HasStateChanged)
        {
            if (_dataProtector == null) throw new NullReferenceException(nameof(_dataProtector));

            lock (FileLock)
            {
                // reflect changes in the persistent store
                Console.WriteLine("MacTokenCacheHelper:AfterAccessNotification-Attempt to write token cache file started");
                try
                {
                    File.WriteAllBytes(
                        CacheFilePath,
                        _dataProtector.Protect(args.TokenCache.SerializeMsalV3())
                        //ProtectedData.Protect(
                        //    args.TokenCache.SerializeMsalV3(),
                        //    null,
                        //    DataProtectionScope.CurrentUser
                        //)
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MacTokenCacheHelper:AfterAccessNotification-Exception writing token cache - {0}", ex);
                }
            }
            Console.WriteLine("MacTokenCacheHelper:AfterAccessNotification-Attempt to write token cache file complete");
        }
        else
        {
            Console.WriteLine("MacTokenCacheHelper:AfterAccessNotification-Write token cache not required");
        }
    }
}