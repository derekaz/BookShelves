using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Shared.Tests;

public sealed class RazorProgramExtensionsTests
{
    [Fact]
    public void AddRazorClassLibraryServices_WithoutConfig_RegistersPageSyncCoordinatorAsScoped()
    {
        var services = new ServiceCollection();

        var returned = services.AddRazorClassLibraryServices();

        Assert.Same(services, returned);
        var descriptor = Assert.Single(services, x => x.ServiceType == typeof(IPageSyncCoordinator));
        Assert.Equal(typeof(PageSyncCoordinator), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddRazorClassLibraryServices_WithConfig_RegistersPageSyncCoordinatorAsScoped()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var returned = services.AddRazorClassLibraryServices(configuration);

        Assert.Same(services, returned);
        var descriptor = Assert.Single(services, x => x.ServiceType == typeof(IPageSyncCoordinator));
        Assert.Equal(typeof(PageSyncCoordinator), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
