using System;
#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif IPHONE
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace NeonShooter
{
#if MONOMAC
	class Program
	{
		static void Main (string[] args)
		{
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate ();

				// Set our Application Icon
				NSImage appIcon = NSImage.ImageNamed ("monogameicon.png");
				NSApplication.SharedApplication.ApplicationIconImage = appIcon;
				
				NSApplication.Main (args);
			}
		}
	}

	class AppDelegate : NSApplicationDelegate
	{
		private NeonShooterGame game;

		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			game = new NeonShooterGame();
			game.Run();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}
#elif IPHONE
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		private NeonShooterGame game;

        internal static void RunGame()
        {
            game = new NeonShooterGame();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
	}
#else
		/// <summary>
		/// The main class.
		/// </summary>
		public static class Program
		{
			/// <summary>
			/// The main entry point for the application.
			/// </summary>
			static void Main()
			{
	#if WINDOWS || LINUX || PSM || NETCOREAPP
				using (var game = new NeonShooterGame())
					game.Run();

	#else
				var factory = new MonoGame.Framework.GameFrameworkViewSource<NeonShooterGame>();
				Windows.ApplicationModel.Core.CoreApplication.Run(factory);
	#endif
			}
		}
#endif

}
