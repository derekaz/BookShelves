
using Microsoft.AspNetCore.Components;
using System.Net;

namespace BookShelves.Web.Client.Services;

public class BlazorAuthorizationHandler : DelegatingHandler
{
    private readonly NavigationManager _navigation;

    public BlazorAuthorizationHandler(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Console.WriteLine("Redirecting to login!");

            // Redirect the user to the server's login endpoint, passing the current page as the return URL
            var returnUrl = Uri.EscapeDataString(_navigation.Uri);

            // Construct an absolute URI to break past Blazor's client-side router
            var absoluteChallengeUrl = _navigation.ToAbsoluteUri($"/MicrosoftIdentity/Account/Challenge?returnUrl={returnUrl}").ToString();


            _navigation.NavigateTo($"account/login?returnUrl={returnUrl}", forceLoad: true);
            //_navigation.NavigateTo($"MicrosoftIdentity/Account/SignIn?returnUrl={returnUrl}", forceLoad: true);
            //_navigation.NavigateTo($"MicrosoftIdentity/Account/Challenge?returnUrl={returnUrl}", forceLoad: true);
            // forceLoad: true explicitly forces a complete browser reload to the server
            //_navigation.NavigateTo(absoluteChallengeUrl, forceLoad: true);
        }

        return response;
    }
}