using System;
using Foundation;
using UIKit;

namespace DungeonSlime.iOS
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static Game1 game;

        internal static void RunGame()
        {
            try
            {
                game = new Game1();
                game.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game failed to start: {ex}");
                throw;
            }
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