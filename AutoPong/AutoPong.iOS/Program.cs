using Foundation;
using UIKit;

namespace AutoPong.iOS
{
    [Register("AppDelegate")]
    internal class Program : UIApplicationDelegate
    {
        private static AutoPongGame game;

        internal static void RunGame()
        {
            game = new AutoPongGame();
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