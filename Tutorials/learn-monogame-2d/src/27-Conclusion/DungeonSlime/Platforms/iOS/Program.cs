using System;
using Foundation;
using UIKit;

namespace DungeonSlime.iOS
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static DungeonSlimeGame game;

        internal static void RunGame()
        {
            game = new DungeonSlimeGame();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(Program));
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
    }
}