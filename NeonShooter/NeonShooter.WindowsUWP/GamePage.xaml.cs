using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NeonShooter.WindowsUWP
{
    public sealed partial class GamePage : Page
    {
        readonly NeonShooterGame _game;

        public GamePage()
        {
            this.InitializeComponent();

            // Create the game.
            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<NeonShooterGame>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
