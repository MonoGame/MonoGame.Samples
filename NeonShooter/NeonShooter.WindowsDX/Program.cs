using System;

namespace NeonShooter.WindowsDX
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
