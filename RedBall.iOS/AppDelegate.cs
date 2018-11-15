using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace RedBall.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            LaunchGame();
            return true;
        }

        async void LaunchGame()
        {
            await Task.Yield();
            new RedBallGame().Run();
        }
    }
}