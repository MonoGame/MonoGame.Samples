using System;
using Platformer2D.Core;

internal class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// This creates an instance of your game and calls it's Run() method
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                // On macOS, SDL2 requires specific configuration
                Environment.SetEnvironmentVariable("SDL_VIDEO_MAC_FULLSCREEN_SPACES", "0");
            }

            using var game = new Platformer2DGame();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}