using BookShelves.Shared.Services.AuthorizationPolicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace BookShelves.Shared.Tests.Services;

public sealed class AuthorizationPoliciesTests
{
    [Fact]
    public void AddAppAuthorizationPolicies_AddsAdminAndAuthenticatedPolicies()
    {
        var options = new AuthorizationOptions();

        options.AddAppAuthorizationPolicies();

        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.AdminAccess));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.Authenticated));
    }

    [Fact]
    public void AdminAccessPolicy_RequiresAdministratorRole()
    {
        var options = new AuthorizationOptions();
        options.AddAppAuthorizationPolicies();

        var policy = options.GetPolicy(AuthorizationPolicies.AdminAccess);

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, requirement => requirement is RolesAuthorizationRequirement);
    }
}
