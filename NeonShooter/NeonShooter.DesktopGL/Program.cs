using System;

namespace NeonShooter.DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new NeonShooterGame())
                game.Run();
        }
    }
}
