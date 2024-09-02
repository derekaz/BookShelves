using BookShelves.Maui.Data;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using BookShelves.Shared;
using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace BookShelves.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            });


        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif


        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
                events.AddAndroid(platform =>
                {
                    platform.OnActivityResult((activity, rc, result, data) =>
                    {
                        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(rc, result, data);
                    });
                });
#endif
        });

#if ANDROID
        builder.Services.AddSingleton<IWindowService, Platforms.Android.WindowService>();
#elif IOS
        builder.Services.AddSingleton<IWindowService, Platforms.IOS.WindowService>();
#elif MACCATALYST
        builder.Services.AddSingleton<IWindowService, Platforms.Mac.WindowService>();
#elif WINDOWS
        builder.Services.AddSingleton<IWindowService, Platforms.Windows.WindowService>();
#endif

        builder.Services.AddOptions();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        //var baseUrl = string.Join("/",
        //    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"],
        //    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
        //var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
        //    .Get<List<string>>();
        //builder.Services.AddGraphClient(baseUrl, scopes);

        var assembly = Assembly.GetExecutingAssembly();
        var appName = assembly.GetName().Name;

        using var appSettingsStream = assembly.GetManifestResourceStream($"{appName}.appSettings.json");
        using var appSettingsDevStream = assembly.GetManifestResourceStream($"{appName}.appSettings.Development.json");

        var configBuilder = new ConfigurationBuilder();

        // Add appSettings.json to configuration
        if (appSettingsStream != null) configBuilder.AddJsonStream(appSettingsStream);

        // Optionally use appSettings.Development.json to override values in
        // appSettings.json that shouldn't be committed to source control
        if (appSettingsDevStream != null) configBuilder.AddJsonStream(appSettingsDevStream);

        //if (configBuilder.Sources.Count > 0) builder.Configuration.AddConfiguration(configBuilder.Build());
        var config = configBuilder.Build();

        //var config = new ConfigurationBuilder()
        //            .AddJsonFile(new EmbeddedFileProvider(assembly), "appsettings.json", optional: true, false)
        //            //.AddJsonStream(stream)
        //            .Build();

        //      try
        //      {
        builder.Configuration.AddConfiguration(config);
        builder.Services.AddMauiBlazorWebView();
        //      }
        //      catch (Exception ex) 
        //{
        //	Debug.WriteLine(ex);
        //}
#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddSingleton<IVersionService, VersionService>();

        var dbPath = FileAccessHelper.GetLocalFilePath(Constants.LocalDbFile);

        builder.Services.AddSingleton<IDataService>(
            s => ActivatorUtilities.CreateInstance<DataService>(s, dbPath));

        builder.Services.AddScoped<AuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();
        builder.Services.AddSingleton<IBooksDataService, BooksDataService>();
        builder.Services.AddTransient<HttpClient>();
        builder.Services.AddSingleton<IMacAuthenticationService, MacAuthenticationService>();

        builder.Services.AddRazorClassLibraryServices(config);

#if WINDOWS
        WinUIEx.WebAuthenticator.CheckOAuthRedirectionActivation();
#else
        //WebAuthenticator.CheckOAuthRedirectionActivation();
#endif

#if MACCATALYST
        try
        {
            string dataProtectionKeysDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MacOsEncryption-Keys");
            X509Certificate2 dataProtectionCertificate = SetupDataProtectionCertificate();
            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Certificate Setup Complete-Cert:{0}; {1}; {2}", dataProtectionCertificate.FriendlyName, dataProtectionCertificate.SubjectName, dataProtectionCertificate.SerialNumber);
            X509Certificate2 dataProtectionCertificate2 = SetupDataProtectionCertificate2();
            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Certificate 2 Setup Complete-Cert:{0}; {1}; {2}", dataProtectionCertificate2.FriendlyName, dataProtectionCertificate2.SubjectName, dataProtectionCertificate2.SerialNumber);

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDirectory))
                .ProtectKeysWithCertificate(dataProtectionCertificate2);

            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Build Key Configuration Setup Complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Build Exception - {0}", ex);
        }

#endif

        try
        {
            return builder.Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:CreateMauiApp-Build Exception-{0}", ex.ToString());
            throw;
        }
    }

#if MACCATALYST
    static X509Certificate2 CreateSelfSignedDataProtectionCertificate(string subjectName)
    {
        Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Creation Started - SubjectName:{0}", subjectName);
        try
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                X509Certificate2 ephemeral = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(5));
                Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Created Ephemeral - SubjectName:{0}", ephemeral.SubjectName);
                //using (ephemeral)
                //{
                //    X509Certificate2 certificate = new X509Certificate2(
                //        ephemeral.Export(X509ContentType.Pkcs12),
                //        string.Empty,
                //        //X509KeyStorageFlags.PersistKeySet
                //        X509KeyStorageFlags.Exportable
                //    );

                //    Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Creation Complete - Cert:{0}; {1}; {2}", certificate.FriendlyName, certificate.SubjectName, certificate.SerialNumber);
                //    return certificate;
                //}
                return ephemeral;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Exception - {0}", ex);
            throw;
        }
    }

    static void InstallCertificateAsNonExportable(X509Certificate2 certificate)
    {
        Console.WriteLine("MauiProgram:InstallCertificateAsNonExportable - Install Started - Cert:'{0}'/'{1}' ({2})", certificate.FriendlyName, certificate.SubjectName, certificate.SerialNumber);
        byte[] rawData = certificate.Export(X509ContentType.Pkcs12, password: "");

        using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser)) //, OpenFlags.ReadWrite))
        {
            try
            {
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                Console.WriteLine("MauiProgram:InstallCertificateAsNonExportable - Attempting to install - {0}", rawData.ToString());
                store.Certificates.Import(rawData, password: "", keyStorageFlags: X509KeyStorageFlags.PersistKeySet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MauiProgram:InstallCertificateAsNonExportable - Store Certificate Exception - {0}", ex);
                throw;
            }
            finally
            {
                store.Close();
                Console.WriteLine("MauiProgram:InstallCertificateAsNonExportable - Install Completed");
            }
        }
    }

    static void SaveCertificateToFile(X509Certificate2 certificate, string filePath, string password)
    {
        Console.WriteLine("MauiProgram:SaveCertificateToFile - Start - Cert:'{0}'/'{1}' ({2}) File:{3}", certificate.FriendlyName, certificate.SubjectName, certificate.SerialNumber, filePath);
        if (File.Exists(filePath)) 
        {
            Console.WriteLine("MauiProgram:SaveCertificateToFile - Deleting Existing - File:{0}", filePath);
            File.Delete(filePath); 
        }

        File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Pkcs12, password));
        Console.WriteLine("MauiProgram:SaveCertificateToFile - Written - File:{0}", filePath);
    }

    static string GetPasswordFromStore()
    {
        string storageKey = "BookShelvesEncryptionCertificateKey";
        var storedValue = SecureStorage.GetAsync(storageKey).Result;

        if (storedValue == null)
        {
            var rng = new Random();
            var randomString = rng.NextStrings(RandomExtensions.AllowableRandomStringCharacters, (15, 64), 1).First();
            SecureStorage.SetAsync(storageKey, randomString);
            return randomString;
        }

        return storedValue;
    }

    static X509Certificate2 SetupDataProtectionCertificate2()
    {
        Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Started");
        try
        {
            string subjectName = "CN=BooKShelves ASP.NET Core Data Protection Certificate";
            string dataProtectionCertDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DataProtectionCert.pfx");

            var certPassword = GetPasswordFromStore();

            if (File.Exists(dataProtectionCertDirectory))
            {
                X509Certificate2 cert = new X509Certificate2(dataProtectionCertDirectory, certPassword);
                if (cert != null && cert.Subject == subjectName && DateTime.Now <= cert.NotAfter)
                {
                    Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Complete - Found existing file");
                    return cert;
                }
                Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - File not found");
            }

            X509Certificate2 certificate = CreateSelfSignedDataProtectionCertificate(subjectName);
            SaveCertificateToFile(certificate, dataProtectionCertDirectory, certPassword);
            Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Complete - Created new certificate");

            return certificate;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Exception - {0}", ex);
            throw;
        }
    }

    static X509Certificate2 SetupDataProtectionCertificate()
    {
        Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - Setup Started");
        string subjectName = "CN=BooKShelves ASP.NET Core Data Protection Certificate";
        using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser)) //, OpenFlags.ReadOnly))
        {
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                foreach (var cert in store.Certificates)
                {
                    Console.WriteLine("  Cert from store - '{0}' ({1})", cert.SubjectName, cert.SerialNumber);
                }

                X509Certificate2Collection certificateCollection = store.Certificates.Find(X509FindType.FindBySubjectName,
                    subjectName,
                    // self-signed certificate won't pass X509 chain validation
                    validOnly: false);
                if (certificateCollection.Count > 0)
                {
                    Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - Setup Complete - Found in store");
                    return certificateCollection[0];
                }
                else
                {
                    Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - None found in store");
                }

                X509Certificate2 certificate = CreateSelfSignedDataProtectionCertificate(subjectName);
                InstallCertificateAsNonExportable(certificate);
                Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - Setup Complete - Created new certificate");
                return certificate;

            }
            catch (Exception ex)
            {
                Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - Exception - {0}", ex);
                throw;
            }
            finally
            { 
                store.Close();
            }
        }
    }
#endif
}
