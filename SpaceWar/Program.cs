using System;

namespace Spacewar
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
            using (var game = new SpacewarGame())
                game.Run();

#else
            var factory = new MonoGame.Framework.GameFrameworkViewSource<SpacewarGame>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
#endif
        }
    }
#endif
}
