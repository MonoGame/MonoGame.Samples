using System;

namespace Platformer2D
{
#if !WINDOWS_PHONE
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
#if WINDOWS || LINUX || PSM
            using (var game = new PlatformerGame())
                game.Run();

#else
            var factory = new MonoGame.Framework.GameFrameworkViewSource<PlatformerGame>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
#endif
        }
    }
#endif
}
