using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services;
using BookShelves.Maui.Data.SyncTest;
using BookShelves.Maui.Handlers;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using BookShelves.Shared;
using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor.Services;
using System.Reflection;

namespace BookShelves.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Thread.Sleep(10000);
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        //AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
        //{
        //    try
        //    {
        //        Console.WriteLine($"[CRITICAL EXCEPTION]: {args.Exception.Message}");
        //        Console.WriteLine(args.Exception.StackTrace);

        //        // Best-effort write to a persistent crash log so very early failures are captured
        //        try
        //        {
        //            var crashPath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "unhandled-crash.log");
        //            File.AppendAllText(crashPath, $"=== FirstChanceException ({DateTime.UtcNow:O}) ===\n{args.Exception}\n\n");
        //        }
        //        catch { }
        //    }
        //    catch { }
        //};

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

        builder.Services.AddMudServices();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
#if DEBUG
            logging.AddDebug();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
#endif
        });

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

        // Add authorization with app-specific policies
        builder.Services.AddAuthorizationCore(options =>
        {
            options.AddAppAuthorizationPolicies();
        });

        builder.Services.AddCascadingAuthenticationState();

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

        var config = configBuilder.Build();

        builder.Configuration.AddConfiguration(config);

        builder.Services.AddSingleton<IFormFactor, MauiFormFactor>();
        builder.Services.AddSingleton<IVersionService, MauiVersionService>();
        builder.Services.AddScoped<IAuthenticationUIProvider, MauiAuthenticationUIProvider>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s => (AuthenticationStateProvider)s.GetRequiredService<IExternalAuthenticationStateProvider>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();

        // sync progress notifier used by UI to present synchronization status
        builder.Services.AddSingleton<ISyncProgressService, SyncProgressService>();
        builder.Services.AddTransient<ISyncUnitOfWork<SyncDbContext>>(sp =>
        {
            var uow = new SyncUnitOfWork<SyncDbContext>(
                sp.GetRequiredService<IDbContextFactory<SyncDbContext>>());
            uow.SyncProgressService = sp.GetRequiredService<ISyncProgressService>();
            return uow;
        });

        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("BooksApi", client =>
        {
            // client.BaseAddress = new Uri("https://bookshelves.cloud.azmoore.com");
            // client.BaseAddress = new Uri("https://green-ground-05694281e-dev013.westus2.2.azurestaticapps.net");
            client.BaseAddress = new Uri("http://localhost:7071");
            client.Timeout = new TimeSpan(0, 0, 20);
        });

        builder.Services.AddHttpClient("WeatherApi", client =>
        {
            string baseUrl = builder.Configuration.GetSection("WeatherApi:BaseUrl").Get<string>() ?? string.Empty;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = new TimeSpan(0, 0, 20);
        }).AddHttpMessageHandler(sp =>
        {
            var scopes = builder.Configuration.GetSection("WeatherApi:Scopes").Get<string[]>() ?? [];
            return new MauiAuthenticationMessageHandler(
                sp.GetRequiredService<IExternalAuthenticationStateProvider>(),
                sp.GetRequiredService<ILogger<MauiAuthenticationMessageHandler>>(),
                scopes);
        })
#if DEBUG
        // .AddTraceContentLogging()
#endif
        ;
        builder.Services.AddHttpClient("SyncApi", client =>
        {
            string baseUrl = builder.Configuration.GetSection("SyncApi:BaseUrl").Get<string>() ?? string.Empty;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = new TimeSpan(0, 0, 20);
        })
        .AddHttpMessageHandler(_ =>
        {
            return new LoggingHandler();
        })
        .AddHttpMessageHandler(sp =>
        {
            var scopes = builder.Configuration.GetSection("SyncApi:Scopes").Get<string[]>() ?? [];
            return new MauiAuthenticationMessageHandler(
                sp.GetRequiredService<IExternalAuthenticationStateProvider>(),
                sp.GetRequiredService<ILogger<MauiAuthenticationMessageHandler>>(),
                scopes);
        });

        // Configure DbContext
        var bsp = builder.Services.BuildServiceProvider();
        var loggerFactory = bsp.GetRequiredService<ILoggerFactory>();

        Data.Extensions.SqliteProviderExtension.RegisterSqliteProvider();

        // builder.Configuration.AddSqliteConfiguration(localDbConnectionString, loggerFactory);

        builder.Services.AddDbContextFactory<BookShelvesDbContext>(options =>
        {
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
#endif

#if DEBUG
            System.Diagnostics.Debug.WriteLine("MauiProgram:CreateMauiApp - Set dbPath:{0}", dbPath);
#endif

            var localDbConnectionString = $"Data Source={dbPath}";

            options.UseSqlite(localDbConnectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        builder.Services.AddTransient<IUnitOfWork<BookShelvesDbContext>, UnitOfWork<BookShelvesDbContext>>();
        builder.Services.AddTransient<IRepository<LocalBook>, GenericRepository<BookShelvesDbContext, LocalBook>>(); // Register specific repositories if needed
        builder.Services.AddTransient<IBooksDataService, BooksDataService>();
        builder.Services.AddTransient<IBookFactory, BookViewModelFactory>();
        builder.Services.AddTransient<IBook, LocalBook>();

        builder.Services.AddTransient<IUnitOfWork<SyncDbContext>, UnitOfWork<SyncDbContext>>();
        builder.Services.AddTransient<IRepository<Author>, GenericRepository<SyncDbContext, Author>>(); // Register specific repositories if needed
        builder.Services.AddTransient<IAuthorDataService, MauiAuthorDataService>();

        builder.Services.AddTransient<ISyncDataService, MauiSyncDataService>();

        // try to utilize the offline sync service
        builder.Services.AddScoped<SyncDbContextInitializer>();
        builder.Services.AddScoped<IDbInitializer, SyncDbContextInitializer>();
        builder.Services.AddDbContextFactory<SyncDbContext>(options =>
        {
            // set the local database path
#if MACCATALYST
            var dbPath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, Constants.LocalDbFile);
            var dbPath2 = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "BookShelvesTest.db");
            if (File.Exists(dbPath2))
            {
                File.Delete(dbPath2);
            }
#else
            // var dbPath = FileAccessHelper.GetLocalFilePath(Constants.LocalDbFile);
            var dbPath = FileAccessHelper.GetLocalFilePath("BookShelvesSyncTest.db");
#endif

#if DEBUG
            System.Diagnostics.Debug.WriteLine("MauiProgram:CreateMauiApp - Set dbPath:{0}", dbPath);
#endif

            var localDbConnectionString = $"Data Source={dbPath}";

            options.UseSqlite(localDbConnectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
        // builder.Services.AddDbContext<AuthorDbContext>(options => options.UseSqlite(localDbConnectionString));


        // builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();

        builder.Services.AddTransient<IWeatherForecaster, MauiWeatherForecaster>();

        //builder.Services.AddHttpLogging(logging =>
        //{
        //    logging.LoggingFields = HttpLoggingFields.All;
        //    logging.RequestHeaders.Add("sec-ch-ua");
        //    logging.ResponseHeaders.Add("MyResponseHeader");
        //    logging.MediaTypeOptions.AddText("application/javascript");
        //    logging.RequestBodyLogLimit = 4096;
        //    logging.ResponseBodyLogLimit = 4096;

        //});

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
#endif

        try
        {
            var app = builder.Build();

            ApplicationLogger.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

            // Register global unhandled exception handlers to capture crashes when running as a packaged app.
            try
            {
                var globalLogger = ApplicationLogger.CreateLogger("GlobalUnhandledException");

                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    try
                    {
                        var ex = args.ExceptionObject as Exception ?? new Exception("Non-Exception thrown to AppDomain.CurrentDomain.UnhandledException");
                        globalLogger.LogCritical(ex, "AppDomain unhandled exception. IsTerminating={IsTerminating}", args.IsTerminating);
                        try
                        {
                            // persist to local file for post-mortem analysis
                            string desktopPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                            string crashLogPath = Path.Combine(desktopPath, "BookShelves-Unhandled-Crash-Log.txt");
                            File.AppendAllText(crashLogPath, $"=== AppDomain UnhandledException ({DateTime.UtcNow:O}) ===\nError: {ex.Message}\nException: {ex}\n");
                        }
                        catch { /* best-effort only */ }
                    }
                    catch { }
                };

                TaskScheduler.UnobservedTaskException += (sender, args) =>
                {
                    try
                    {
                        var ex = args.Exception ?? new AggregateException("UnobservedTaskException without Exception");
                        globalLogger.LogCritical(ex, "Unobserved task exception");
                        try
                        {
                            // persist to local file for post-mortem analysis
                            string desktopPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                            string crashLogPath = Path.Combine(desktopPath, "BookShelves-Unobserved-Crash-Log.txt");
                            File.AppendAllText(crashLogPath, $"=== TaskScheduler UnobservedTaskException ({DateTime.UtcNow:O}) ===\nError: {ex.Message}\nException: {ex}\n");
                        }
                        catch { }
                        args.SetObserved();
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                // if logger factory isn't available or something else fails, fallback to console
                Console.WriteLine("Failed to register global exception handlers: {0}", ex);
            }

            app.Services.GetRequiredService<BookShelvesDbContext>().UpdateDatabase();
            app.Services.GetRequiredService<SyncDbContextInitializer>().Initialize();

            return app;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MauiProgram:CreateMauiApp-Build Exception-{0}", ex.ToString());
            throw;
        }
    }
}
