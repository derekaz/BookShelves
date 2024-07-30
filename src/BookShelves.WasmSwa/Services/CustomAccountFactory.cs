using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Graph;

namespace BookShelves.WasmSwa.Services;

public class CustomAccountFactory(IAccessTokenProviderAccessor accessor,
        IServiceProvider serviceProvider,
        ILogger<CustomAccountFactory> logger)
    : AccountClaimsPrincipalFactory<CustomUserAccount>(accessor)
{
    private readonly ILogger<CustomAccountFactory> logger = logger;
    private readonly IServiceProvider serviceProvider = serviceProvider;

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        CustomUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var initialUser = await base.CreateUserAsync(account, options);

        if (initialUser.Identity is not null &&
            initialUser.Identity.IsAuthenticated)
        {
            var userIdentity = initialUser.Identity as ClaimsIdentity;

            if (userIdentity is not null)
            {
                account?.Roles?.ForEach((role) =>
                {
                    userIdentity.AddClaim(new Claim("roles", role));
                });

                account?.Wids?.ForEach((wid) =>
                {
                    userIdentity.AddClaim(new Claim("directoryRole", wid));
                });

                //try
                //{
                //    var client = ActivatorUtilities
                //        .CreateInstance<GraphServiceClient>(serviceProvider);
                //    var user = await client.Me.GetAsync();
                //    //var request = client.Me. .Request();
                //    //var user = await request.GetAsync();

                //    if (user is not null)
                //    {
                //        userIdentity.AddClaim(new Claim("mobilephone", user.MobilePhone ?? "(000) 000-0000"));
                //        userIdentity.AddClaim(new Claim("officelocation", user.OfficeLocation ?? "Not set"));
                //    }

                //    var requestMemberOf = client.Users[account?.Oid].MemberOf;
                //    //var memberships = await requestMemberOf.Request().GetAsync();
                //    var memberships = await requestMemberOf.GetAsync();

                //    if (memberships?.Value is not null)
                //    {
                //        foreach (var entry in memberships.Value)
                //        {
                //            if (entry.OdataType == "#microsoft.graph.group")
                //            {
                //                userIdentity.AddClaim(
                //                    new Claim("directoryGroup", entry.Id ?? ""));
                //            }
                //        }
                //    }
                //}
                //catch (AccessTokenNotAvailableException exception)
                //{
                //    exception.Redirect();
                //}
            }
        }

        return initialUser;
    }
}