using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Services;

internal class ApplicationLogger
{
    internal static ILoggerFactory? LoggerFactory { get; set; }
    internal static ILogger CreateLogger<T>() => LoggerFactory!.CreateLogger<T>();
    internal static ILogger CreateLogger(string categoryName) => LoggerFactory!.CreateLogger(categoryName);
}
