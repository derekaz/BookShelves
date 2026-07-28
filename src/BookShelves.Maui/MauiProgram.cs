using BookShelves.Maui.Data.Handlers;
using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Interfaces;
using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services.Maui;
using BookShelves.Maui.Handlers;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Interfaces;
using BookShelves.Maui.Services;
using BookShelves.Maui.Services.Maui;
using BookShelves.Shared;
using BookShelves.Shared.Data.Bases;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor.Services;
using Serilog;
using System.Reflection;

namespace BookShelves.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        string startupMilestone = "M00";

        try
        {
            startupMilestone = "M01-CreateMauiApp-Enter";

            // 1. Establish the platform-specific safe logging directory
            var logPath = FileAccessHelper.GetLogFilePath("app-log-.txt");

            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Debug() // Filter out verbose logs for production
#endif
                .WriteTo.Debug()            // Keep for local IDE debugging
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day, // Creates a new log file every day
                    retainedFileCountLimit: 7,            // Keeps only the last 7 days of logs
                    fileSizeLimitBytes: 5_000_000,        // Limit individual file size to 5MB
                    rollOnFileSizeLimit: true)            // Create new file if 5MB is exceeded
                .CreateLogger();

            Log.Information("{Milestone}: Serilog initialized at {LogPath}", startupMilestone, logPath);

            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                try
                {
                    Console.WriteLine($"[CRITICAL EXCEPTION]: {args.Exception.Message}");
                    Console.WriteLine(args.Exception.StackTrace);

                    // Best-effort write to a persistent crash log so very early failures are captured
                    try
                    {
                        var crashPath = FileAccessHelper.GetLogFilePath("unhandled-crash.log");
                        File.AppendAllText(crashPath, $"=== FirstChanceException ({DateTime.UtcNow:O}) ===\n{args.Exception}\n\n");
                    }
                    catch { }
                }
                catch { }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                try
                {
                    var ex = args.ExceptionObject as Exception ?? new Exception("Non-Exception thrown to AppDomain.CurrentDomain.UnhandledException");
                    Log.Fatal(ex, "UNHANDLED APPDOMAIN exception. IsTerminating={IsTerminating}", args.IsTerminating);
                    try
                    {
                        string crashLogPath = FileAccessHelper.GetLogFilePath("BookShelves-Unhandled-Crash-Log.txt");
                        File.AppendAllText(crashLogPath, $"=== AppDomain UnhandledException ({DateTime.UtcNow:O}) ===\nError: {ex.Message}\nException: {ex}\n");
                    }
                    catch { }
                }
                catch { }
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                try
                {
                    var ex = args.Exception ?? new AggregateException("UnobservedTaskException without Exception");
                    Log.Fatal(ex, "UNOBSERVED TASK exception");
                    try
                    {
                        string crashLogPath = FileAccessHelper.GetLogFilePath("BookShelves-Unobserved-Crash-Log.txt");
                        File.AppendAllText(crashLogPath, $"=== TaskScheduler UnobservedTaskException ({DateTime.UtcNow:O}) ===\nError: {ex.Message}\nException: {ex}\n");
                    }
                    catch { }
                    args.SetObserved();
                }
                catch { }
            };

            startupMilestone = "M02-Builder-Create";
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

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(dispose: true);
            Log.Information("{Milestone}: Builder and logging providers configured", startupMilestone);

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddMudServices();
        Log.Information("M03-UI-Services-Registered");

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
#elif IOS
            events.AddiOS(platform =>
            {
                platform.FinishedLaunching((_, _) =>
                {
                    Log.Information("M04-iOS-FinishedLaunching");
                    return true;
                });
                platform.OnActivated(_ => Log.Information("M05-iOS-OnActivated"));
                platform.DidEnterBackground(_ => Log.Information("M06-iOS-DidEnterBackground"));
                platform.WillEnterForeground(_ => Log.Information("M07-iOS-WillEnterForeground"));
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

        var configBuilder = new ConfigurationBuilder();

        // Add appSettings.json to configuration
        using var appSettingsStream = assembly.GetManifestResourceStream($"{appName}.appSettings.json");
        if (appSettingsStream != null) configBuilder.AddJsonStream(appSettingsStream);

#if DEBUG
        using var appSettingsDevStream = assembly.GetManifestResourceStream($"{appName}.appSettings.Development.json");
        // Only apply Development overrides for debug builds
        if (appSettingsDevStream != null) configBuilder.AddJsonStream(appSettingsDevStream);
#endif

        var config = configBuilder.Build();

        builder.Configuration.AddConfiguration(config);
        Log.Information("M08-Configuration-Loaded");

        builder.Services.AddSingleton<IFormFactor, FormFactorService>();
        builder.Services.AddSingleton<IVersionService, VersionService>();
        builder.Services.AddScoped<IAuthenticationUIProvider, AuthenticationUIProviderService>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s => (AuthenticationStateProvider)s.GetRequiredService<IExternalAuthenticationStateProvider>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();

        // sync progress notifier used by UI to present synchronization status
        builder.Services.AddSingleton<ISyncProgressService, SyncProgressService>();
        builder.Services.AddTransient<ISyncUnitOfWork<SyncDbContext>, SyncUnitOfWork<SyncDbContext>>();

        //builder.Services.AddHttpClient();
        //builder.Services.AddHttpClient("BooksApi", client =>
        //{
        //    // client.BaseAddress = new Uri("https://bookshelves.cloud.azmoore.com");
        //    // client.BaseAddress = new Uri("https://green-ground-05694281e-dev013.westus2.2.azurestaticapps.net");
        //    client.BaseAddress = new Uri("http://localhost:7071");
        //    client.Timeout = new TimeSpan(0, 0, 20);
        //});

        builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
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
        .AddTraceContentLogging()
#endif
        ;

        builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>(client =>
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
        })
#if DEBUG
        .AddTraceContentLogging()
#endif
        ;

        // Configure DbContext
        var bsp = builder.Services.BuildServiceProvider();
        var loggerFactory = bsp.GetRequiredService<ILoggerFactory>();

        Data.Extensions.SqliteProviderExtension.RegisterSqliteProvider();

        // builder.Configuration.AddSqliteConfiguration(localDbConnectionString, loggerFactory);

        //        builder.Services.AddDbContextFactory<BookShelvesDbContext>(options =>
        //        {
        //            // set the local database path
        //#if MACCATALYST
        //            var dbPath = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, Constants.LocalDbFile);
        //            var dbPath2 = FileAccessHelper.GetLocalFilePath(FileAccessHelper.ApplicationSubPath, true, "BookShelvesTest.db");
        //            if (File.Exists(dbPath2))
        //            {
        //                File.Delete(dbPath2);
        //            }
        //#else
        //            var dbPath = FileAccessHelper.GetLocalFilePath("bookshelves.db");
        //#endif

        //#if DEBUG
        //            System.Diagnostics.Debug.WriteLine("MauiProgram:CreateMauiApp - Set dbPath:{0}", dbPath);
        //#endif

        //            var localDbConnectionString = $"Data Source={dbPath}";

        //            options.UseSqlite(localDbConnectionString);
        //            options.EnableSensitiveDataLogging();
        //            options.EnableDetailedErrors();
        //        });

        builder.Services.AddTransient<IUnitOfWork<SyncDbContext>, UnitOfWork<SyncDbContext>>();

        builder.Services.AddTransient<IRepository<Author>, GenericRepository<SyncDbContext, Author>>();
        builder.Services.AddTransient<IAuthorsDataService, AuthorDataService>();

        builder.Services.AddTransient<IRepository<Book>, GenericRepository<SyncDbContext, Book>>();
        builder.Services.AddTransient<IBooksDataService, BookDataService>();

        builder.Services.AddTransient<ISyncDataService, SyncDataService>();

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

            options.UseSqlite(localDbConnectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly("BookShelves.Maui.Data");
            });

            options.LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                new[] { DbLoggerCategory.Database.Command.Name }, // Filters down to just SQL queries/commands
                Microsoft.Extensions.Logging.LogLevel.Information);

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
        // builder.Services.AddDbContext<AuthorDbContext>(options => options.UseSqlite(localDbConnectionString));


        // builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();

        builder.Services.AddTransient<IWeatherForecasterService, WeatherForecasterService>();

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
        Log.Information("M09-Service-Registration-Complete");

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

        startupMilestone = "M10-Before-Build";
        Log.Information("{Milestone}", startupMilestone);

        var app = builder.Build();

        startupMilestone = "M11-After-Build";
        Log.Information("{Milestone}", startupMilestone);

        ApplicationLogger.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        //app.Services.GetRequiredService<BookShelvesDbContext>().UpdateDatabase();
        startupMilestone = "M12-Before-SyncDbContextInitializer";
        Log.Information("{Milestone}", startupMilestone);
        app.Services.GetRequiredService<SyncDbContextInitializer>().Initialize();

        startupMilestone = "M13-Startup-Complete";
        Log.Information("{Milestone}", startupMilestone);

        return app;
    }
    catch (Exception ex)
    {
        try
        {
            Log.Fatal(ex, "CreateMauiApp failed at {Milestone}", startupMilestone);
            string crashLogPath = FileAccessHelper.GetLogFilePath("BookShelves-Startup-Crash-Log.txt");
            File.AppendAllText(crashLogPath, $"=== CreateMauiApp Failure ({DateTime.UtcNow:O}) ===\nMilestone: {startupMilestone}\nException: {ex}\n\n");
            Log.CloseAndFlush();
        }
        catch
        {
            Console.WriteLine("MauiProgram:CreateMauiApp fatal failure at milestone {0} - {1}", startupMilestone, ex);
        }

        throw;
    }
}
}
