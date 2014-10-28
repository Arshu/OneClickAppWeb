using System;
using System.Collections.Generic;

using Foundation;
using UIKit;

namespace App.Web
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		#region Variables

		// class-level declarations
		UIWindow window;
		WebViewController viewController;

		#endregion

		#region Overrides

		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Console.WriteLine ("FinishedLaunching - Init WebView Controller");
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			viewController = new WebViewController ();
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
						
			return true;
		}

		//applicationWillResignActive followed by applicationDidEnterBackground when the homebutton is pressed or close the Smart Cover
		public override void OnResignActivation (UIApplication application)
		{
			Console.WriteLine ("OnResignActivation - Stop Web Grid");
			if (viewController != null) {
				viewController.StopWebGrid ();
			}
		}

		public override void DidEnterBackground (UIApplication application)
		{
			Console.WriteLine ("DidEnterBackground - Stop Web Grid");
			if (viewController != null) {
				viewController.StopWebGrid ();
			}           
		}

		//applicationWillEnterForeground and applicationDidBecomeActive when the app becomes active
		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("WillEnterForeground - Start Web Grid");
		}

		public override void OnActivated (UIApplication application)
		{
			Console.WriteLine ("OnActivated - Start Web Grid");
			if (viewController != null) {
				viewController.StartWebGrid ();
			}            
		}

		#endregion

		#region Open Url Delegate

		public override bool OpenUrl (UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			Console.WriteLine ("Invoked with OpenUrl: {0}", url.AbsoluteString);
			if (viewController != null) {
				viewController._appUri = url;
			}
			//NSNotificationCenter.DefaultCenter.PostNotificationName("OpenUrl", url);
			return true;
		}

		//NSNotificationCenter.DefaultCenter.AddObserver ("OpenUrl", OnOpenUrl);
		//private void OnOpenUrl(NSNotification notification)
		//{
		//    _fileUrl = (NSUrl)notification.Object;
		//}

		#endregion

		#region Local Notification

		//public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
		//{
		//    base.ReceivedLocalNotification(application, notification);
		//}

		#endregion
	}
}

