using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace BookShelves.Maui.Platforms.Android;

[Activity(Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataHost = "auth",
    DataScheme = "msal1429bc60-21a6-4f87-98a5-27016b33f86a")]
    //DataScheme = "msal{client-id}")]
public class MsalActivity : BrowserTabActivity
{
}
