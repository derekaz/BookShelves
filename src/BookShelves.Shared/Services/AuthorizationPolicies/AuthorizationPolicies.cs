using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookShelves.Shared.Services.AuthorizationPolicies;

/// <summary>
/// Centralized authorization policy definitions for the application.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy name for admin access.
    /// </summary>
    public const string AdminAccess = "AdminAccess";

    /// <summary>
    /// Policy name for authenticated users.
    /// </summary>
    public const string Authenticated = "Authenticated";

    /// <summary>
    /// Role names that confer admin access.
    /// </summary>
    public static readonly string[] AdminRoles = ["Administrator", "administrators"];

    /// <summary>
    /// Adds application-specific authorization policies to the AuthorizationOptions.
    /// This method should be called from both Web and MAUI startup configurations
    /// to ensure consistent policy definitions across all projects.
    /// </summary>
    /// <param name="options">The AuthorizationOptions to configure.</param>
    public static void AddAppAuthorizationPolicies(this AuthorizationOptions options)
    {
        // AdminAccess policy: Requires authenticated user with Admin role
        options.AddPolicy(AdminAccess, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AdminRoles);

            // Alternative: Use claims instead of roles
            // policy.RequireClaim("role", "Admin");
            // policy.RequireClaim("AdminAccess", "true");
        });

        // Authenticated policy: Basic authenticated user requirement
        options.AddPolicy(Authenticated, policy =>
        {
            policy.RequireAuthenticatedUser();
        });

        // Add additional policies as needed:
        // options.AddPolicy("UserAccess", policy => policy.RequireRole("User", "Admin"));
        // options.AddPolicy("ApiAccess", policy => policy.RequireClaim("scope", "api.access"));
    }

    public static bool IsAdminUser(ClaimsPrincipal? user)
    {
        return user?.Identity?.IsAuthenticated == true && AdminRoles.Any(user.IsInRole);
    }
}
