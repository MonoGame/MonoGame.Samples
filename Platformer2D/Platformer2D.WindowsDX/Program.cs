using System;

namespace Platformer2D.WindowsDX
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PlatformerGame())
                game.Run();
        }
    }
}
