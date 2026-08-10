using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Shared;

public static class RazorProgramExtensions
{
    public static IServiceCollection AddRazorClassLibraryServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IPageSyncCoordinator, PageSyncCoordinator>();
        return serviceCollection;
    }

    public static IServiceCollection AddRazorClassLibraryServices(this IServiceCollection serviceCollection, IConfiguration config)
    {
        serviceCollection.AddScoped<IPageSyncCoordinator, PageSyncCoordinator>();
        return serviceCollection;
    }
}
