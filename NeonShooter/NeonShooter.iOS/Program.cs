using System;
using Foundation;
using UIKit;

namespace NeonShooter.iOS
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static NeonShooterGame game;

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
}
