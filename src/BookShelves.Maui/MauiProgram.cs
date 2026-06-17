using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using BookShelves.Shared;
using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
// using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;
// using System;
using System.Reflection;
// using System.Security.Cryptography;
// using System.Security.Cryptography.X509Certificates;

namespace BookShelves.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        BookShelvesDbContext.Initialize();

        // Thread.Sleep(10000);
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
        {
            Console.WriteLine($"[CRITICAL EXCEPTION]: {args.Exception.Message}");
            Console.WriteLine(args.Exception.StackTrace);
        };

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

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
#if DEBUG
            logging.AddDebug();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
#endif
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        // builder.UseWebAuthentication();
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

        // Add authorization with app-specific policies
        builder.Services.AddAuthorizationCore(options =>
        {
            options.AddAppAuthorizationPolicies();
        });

        builder.Services.AddCascadingAuthenticationState();

        var assembly = Assembly.GetExecutingAssembly();
        var appName = assembly.GetName().Name;

        //var baseUrl = string.Join("/",
        //    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"],
        //    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
        //var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
        //    .Get<List<string>>();
        //builder.Services.AddGraphClient(baseUrl, scopes);

        // set the local database path
#if MACCATALYST
        var dbPath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, Constants.LocalDbFile);
        var dbPath2 = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "BookShelvesTest.db");
        if (File.Exists(dbPath2))
        {
            File.Delete(dbPath2);
        }
#else
        var dbPath = FileAccessHelper.GetLocalFilePath("bookshelves.db");

        //var oldDbPath = FileAccessHelper.GetLocalFilePath("bookshelvestest.db");
        //if (File.Exists(oldDbPath))
        //{
        //    File.Delete(oldDbPath);
        //}
#endif

#if DEBUG
        System.Diagnostics.Debug.WriteLine("MauiProgram:CreateMauiApp - Set dbPath:{0}", dbPath);
#endif

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

        //      try
        //      {

        builder.Configuration.AddConfiguration(config);

        //      }
        //      catch (Exception ex) 
        //{
        //	Debug.WriteLine(ex);
        //}

        builder.Services.AddSingleton<IFormFactor, MauiFormFactor>();
        builder.Services.AddSingleton<IVersionService, MauiVersionService>();
        builder.Services.AddScoped<IAuthenticationUIProvider, MauiAuthenticationUIProvider>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s => (AuthenticationStateProvider)s.GetRequiredService<IExternalAuthenticationStateProvider>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();

        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("BooksApi", client =>
        {
            // client.BaseAddress = new Uri("https://bookshelves.cloud.azmoore.com");
            // client.BaseAddress = new Uri("https://green-ground-05694281e-dev013.westus2.2.azurestaticapps.net");
            client.BaseAddress = new Uri("http://localhost:7071");
            client.Timeout = new TimeSpan(0, 0, 20);
        })
#if DEBUG
        // .AddTraceContentLogging();
#endif
        ;

        // Configure DbContext
        var bsp = builder.Services.BuildServiceProvider();
        var loggerFactory = bsp.GetRequiredService<ILoggerFactory>();

        var localDbConnectionString = $"Data Source={dbPath}";

        builder.Configuration.AddSqliteConfiguration(localDbConnectionString, loggerFactory);

        builder.Services.AddDbContext<BookShelvesDbContext>(
            options => options.UseSqlite(localDbConnectionString), ServiceLifetime.Transient);
        builder.Services.AddTransient<IUnitOfWork<LocalBook>, UnitOfWork>();
        builder.Services.AddTransient<IRepository<LocalBook>, GenericRepository<BookShelvesDbContext, LocalBook>>(); // Register specific repositories if needed
        builder.Services.AddTransient<IBooksDataService, BooksDataService>();
        builder.Services.AddTransient<IBookFactory, LocalBookFactory>();
        builder.Services.AddTransient<IBook, LocalBook>();
        builder.Services.AddTransient<IWeatherForecaster, WeatherForecastsDataService>();

        builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();

        //builder.Services.AddHttpLogging(logging =>
        //{
        //    logging.LoggingFields = HttpLoggingFields.All;
        //    logging.RequestHeaders.Add("sec-ch-ua");
        //    logging.ResponseHeaders.Add("MyResponseHeader");
        //    logging.MediaTypeOptions.AddText("application/javascript");
        //    logging.RequestBodyLogLimit = 4096;
        //    logging.ResponseBodyLogLimit = 4096;

        //});

        // builder.Services.AddTransient<HttpClient>();

        builder.Services.AddRazorClassLibraryServices(config);

#if MACCATALYST
        string dataProtectionCertFile = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "DataProtectionCert.pfx");
        if (File.Exists(dataProtectionCertFile))
        {
            File.Delete(dataProtectionCertFile);
        }

        string dataProtectionKeysDirectory = FileAccessHelper.GetLocalFilePath(Path.Combine(FileAccessHelper.ApplicationSubPath, "MacOsEncryption-Keys"), true);
        if (Directory.Exists(dataProtectionKeysDirectory))
        {
            Directory.Delete(dataProtectionKeysDirectory, true);
        }

        //        try
        //        {
        //            string dataProtectionKeysDirectory = FileAccessHelper.GetLocalFilePath(Path.Combine(FileAccessHelper.ApplicationSubPath, "MacOsEncryption-Keys"), true);
        //            X509Certificate2 dataProtectionCertificate = SetupDataProtectionCertificate();
        //            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Certificate Setup Complete-Cert:{0}; {1}; {2}", dataProtectionCertificate.FriendlyName, dataProtectionCertificate.SubjectName, dataProtectionCertificate.SerialNumber);
        //            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection KeysDirectory:{0}", dataProtectionKeysDirectory);

        //            builder.Services.AddDataProtection()
        //                .SetApplicationName("BookShelves")
        //                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDirectory))
        //                .ProtectKeysWithCertificate(dataProtectionCertificate);

        //            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Build Key Configuration Setup Complete");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("MauiProgram:CreateMauiApp - Data Protection Build Exception - {0}", ex);
        //        }

#endif

        try
        {
            var app = builder.Build();

            ApplicationLogger.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            app.Services.GetRequiredService<BookShelvesDbContext>().UpdateDatabase();

            return app;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:CreateMauiApp-Build Exception-{0}", ex.ToString());
            throw;
        }
    }

    // #if MACCATALYST
    //    static X509Certificate2 CreateSelfSignedDataProtectionCertificate(string subjectName)
    //    {
    //        Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Creation Started - SubjectName:{0}", subjectName);
    //        try
    //        {
    //            using RSA rsa = RSA.Create(2048);
    //            CertificateRequest request = new(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    //            X509Certificate2 ephemeral = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(5));
    //            Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Created Ephemeral - SubjectName:{0}", ephemeral.SubjectName);

    //            return ephemeral;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("MauiProgram:CreateSelfSignedDataProtectionCertificate - Exception - {0}", ex);
    //            throw;
    //        }
    //    }

    //    static void SaveCertificateToFile(X509Certificate2 certificate, string filePath, string password)
    //    {
    //        Console.WriteLine("MauiProgram:SaveCertificateToFile - Start - Cert:'{0}'/'{1}' ({2}) File:{3}", certificate.FriendlyName, certificate.SubjectName, certificate.SerialNumber, filePath);
    //        if (File.Exists(filePath)) 
    //        {
    //            Console.WriteLine("MauiProgram:SaveCertificateToFile - Deleting Existing - File:{0}", filePath);
    //            File.Delete(filePath); 
    //        }

    //        File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Pkcs12, password));
    //        Console.WriteLine("MauiProgram:SaveCertificateToFile - Written - File:{0}", filePath);
    //    }

    //    static string GetPasswordFromStore()
    //    {
    //        string storageKey = "BookShelvesEncryptionCertificateKey";
    //        var storedValue = SecureStorage.GetAsync(storageKey).Result;

    //        if (storedValue == null)
    //        {
    //            var rng = new Random();
    //            var randomString = rng.NextStrings(RandomExtensions.AllowableRandomStringCharacters, (15, 64), 1).First();
    //            SecureStorage.SetAsync(storageKey, randomString);
    //            return randomString;
    //        }

    //        return storedValue;
    //    }

    //    static X509Certificate2 SetupDataProtectionCertificate()
    //    {
    //        Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Started");
    //        try
    //        {
    //            string subjectName = "CN=BooKShelves ASP.NET Core Data Protection Certificate";
    //            string dataProtectionCertFile = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "DataProtectionCert.pfx");

    //            var certPassword = GetPasswordFromStore();

    //            if (File.Exists(dataProtectionCertFile))
    //            {
    //                X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(dataProtectionCertFile, certPassword);

    //                // Replace the following code block
    //                // X509Certificate2 cert = new X509Certificate2(dataProtectionCertFile, certPassword);

    //                // With this code block
    //                // X509Certificate2 cert = X509CertificateLoader.LoadFromFile(dataProtectionCertFile, certPassword);
    //                if (cert != null && cert.Subject == subjectName && DateTime.Now <= cert.NotAfter)
    //                {
    //                    Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Complete - Found existing file");
    //                    return cert;
    //                }
    //                Console.WriteLine("MauiProgram:SetupDataProtectionCertificate - File not found");
    //            }

    //            X509Certificate2 certificate = CreateSelfSignedDataProtectionCertificate(subjectName);
    //            SaveCertificateToFile(certificate, dataProtectionCertFile, certPassword);
    //            Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Setup Complete - Created new certificate");

    //            return certificate;
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("MauiProgram:SetupDataProtectionCertificate2 - Exception - {0}", ex);
    //            throw;
    //        }
    //    }
    // #endif
}
