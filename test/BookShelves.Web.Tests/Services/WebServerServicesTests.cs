using System.Reflection;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;

namespace BookShelves.Web.Tests.Services;

public sealed class WebServerServicesTests
{
    [Fact]
    public void AuthenticationUiProvider_ReturnsWebLinkEndpoints()
    {
        var sut = CreateInternalInstance<IAuthenticationUIProvider>("BookShelves.Web.Services.Server.AuthenticationUIProviderService");

        Assert.Equal(AuthenticationUIActionType.Link, sut.LoginActionType);
        Assert.Equal("MicrosoftIdentity/Account/SignIn", sut.GetLoginUrl());
        Assert.Equal(AuthenticationUIActionType.Link, sut.LogoutActionType);
        Assert.Equal("MicrosoftIdentity/Account/SignOut", sut.GetLogoutUrl());
        Assert.True(sut.RequiresNavigation);
        Assert.Equal("Web", sut.PlatformName);
    }

    [Fact]
    public void SyncDataService_DefaultContract_IsNoSyncAndThrowsOnServerSync()
    {
        var sut = CreateInternalInstance<ISyncDataService>("BookShelves.Web.Services.Server.SyncDataService");

        Assert.False(sut.SupportsSync);
        Assert.Throws<NotImplementedException>(() => sut.ServerSyncAsync().GetAwaiter().GetResult());
    }

    [Fact]
    public void VersionService_ReturnsVersionInfoWithBuild()
    {
        var sut = CreateInternalInstance<IVersionService>("BookShelves.Web.Services.Server.VersionService");

        var version = sut.GetVersion();

        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version.CurrentVersion));
        Assert.Equal("0", version.CurrentBuild);
    }

    [Fact]
    public void FormFactorService_UsesVersionServiceAndReportsWeb()
    {
        var versionService = new StubVersionService("v-web-test+1");
        var sut = CreateInternalInstance<IFormFactor>("BookShelves.Web.Services.Server.FormFactorService", versionService);

        Assert.Equal("Web", sut.GetFormFactor());
        Assert.Equal("v-web-test+1", sut.GetVersion());
        Assert.False(string.IsNullOrWhiteSpace(sut.GetPlatform()));
    }

    private static T CreateInternalInstance<T>(string fullTypeName, params object[] args)
    {
        var assembly = Assembly.Load("BookShelves.Web");
        var type = assembly.GetType(fullTypeName) ?? throw new InvalidOperationException($"Type '{fullTypeName}' not found.");

        return (T)(Activator.CreateInstance(type, args) ?? throw new InvalidOperationException($"Could not create '{fullTypeName}'."));
    }

    private sealed class StubVersionService(string version) : IVersionService
    {
        public VersionInfo GetVersion() => new() { CurrentVersion = version, CurrentBuild = "0" };
    }
}
