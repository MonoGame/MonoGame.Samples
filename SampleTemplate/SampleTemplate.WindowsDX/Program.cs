using System;

namespace SampleTemplate.WindowsDX
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new SampleTemplateGame())
                game.Run();
        }
    }
}
