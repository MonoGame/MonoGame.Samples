using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace Platformer2D.Android
{
    [Activity(
        Label = "Platformer2D",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden
    )]
    public class Activity1 : AndroidGameActivity
    {
        private PlatformerGame _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _game = new PlatformerGame();
            _view = _game.Services.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();
        }
    }
}
