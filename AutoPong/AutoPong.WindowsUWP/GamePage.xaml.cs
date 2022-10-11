using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AutoPong.WindowsUWP
{
    public sealed partial class GamePage : Page
    {
        readonly AutoPongGame _game;

        public GamePage()
        {
            this.InitializeComponent();

            // Create the game.
            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<AutoPongGame>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }
    }
}
