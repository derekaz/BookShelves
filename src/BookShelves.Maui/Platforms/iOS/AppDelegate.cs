using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace BookShelves.Maui
{
    [Register("AppDelegate")] 
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
        {
            // Temporary broker callback correctness fix: easy rollback by changing `url` back to `null`.
            if (AuthenticationContinuationHelper.IsBrokerResponse(url?.AbsoluteString))
            {
                // Done on different thread to allow return in no time.
                _ = Task.Factory.StartNew(() => AuthenticationContinuationHelper.SetBrokerContinuationEventArgs(url));

                return true;
            }
            else if (!AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url))
            {
                return false;
            }

            return true;

            //return base.OpenUrl(application, url, options);
        }
    }
}
