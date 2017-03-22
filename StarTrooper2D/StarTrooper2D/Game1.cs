using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarTrooper2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StarTrooper2D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        #region Fields

        public static GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }

        ScreenManager screenManager;

        public static GameState state = new GameState();

        List<String> ScreenStateList = new List<String>();

        public static bool ExitGame;

        #endregion

        public Game1()
        {
            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            Content.RootDirectory = "Content";

            Graphics = new GraphicsDeviceManager(this);
            Graphics.HardwareModeSwitch = false;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Window.AllowUserResizing = false;
            Graphics.PreferredBackBufferWidth = GameConstants.BackBufferWidth;
            Graphics.PreferredBackBufferHeight = GameConstants.BackBufferHeight;
            Graphics.IsFullScreen = true;
            Graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);


            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            AppStart();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void AppStart()
        {
            GameScreen[] StartScreens = new GameScreen[2];
            StartScreens[0] = new BackgroundScreen();
            StartScreens[1] = new MainMenuScreen();
            Texture2D LoadingTexture = Content.Load<Texture2D>(@"MenuAssets\SplashScreenImage");
            LoadingScreen.Load(screenManager, 2, LoadingTexture, PlayerIndex.One, StartScreens);
        }

        //#region Tombstoning

        //private void PhoneApplicationService_Activated(object sender, ActivatedEventArgs e)
        //{
        //    // execute any logic required after activation
        //    if (!e.IsApplicationInstancePreserved)
        //    {
        //        GameActivated();
        //    }
        //}

        //private void GameActivated()
        //{
        //    if (PhoneApplicationService.Current.State.ContainsKey("MenuState"))
        //    {
        //        ScreenStateList = PhoneApplicationService.Current.State["MenuState"] as List<String>;
        //        foreach (String screenstate in ScreenStateList)
        //        {
        //            Type screenType = Type.GetType(screenstate);
        //            GameScreen screen = Activator.CreateInstance(screenType) as GameScreen;
        //            screenManager.AddScreen(screen, PlayerIndex.One);
        //            Debug.WriteLine("XNA Screen {0}", screenstate);

        //        }
        //        PhoneApplicationService.Current.State.Remove("MenuState");
        //        Debug.WriteLine("XNA Screenstates Loaded");

        //        //Get Game Save State Here
        //        if (PhoneApplicationService.Current.State.ContainsKey("GameSaveState"))
        //        {
        //            state = PhoneApplicationService.Current.State["GameSaveState"] as GameState;
        //            PhoneApplicationService.Current.State.Remove("GameSaveState");

        //        }
        //    }
        //    else
        //    {
        //        AppStart();
        //    }
        //}

        //protected override void OnDeactivated(object sender, EventArgs args)
        //{
        //    if (screenManager.CurrentScreenName() == "StarTrooper2DplayScreen")
        //    {
        //        screenManager.AddScreen(new PauseGameScreen(), PlayerIndex.One);

        //    }

        //    foreach (GameScreen screen in screenManager.GetScreens())
        //    {
        //        if (screen.IsSerializable)
        //        {
        //            ScreenStateList.Add(screen.GetType().AssemblyQualifiedName);

        //        }
        //    }
        //    if (PhoneApplicationService.Current.State.ContainsKey("MenuState"))
        //    {
        //        PhoneApplicationService.Current.State.Remove("MenuState");
        //    }
        //    PhoneApplicationService.Current.State.Add("MenuState", ScreenStateList);
        //    //Save Game save state Here
        //    if (PhoneApplicationService.Current.State.ContainsKey("GameSaveState"))
        //    {
        //        PhoneApplicationService.Current.State.Remove("GameSaveState");
        //    }
        //    PhoneApplicationService.Current.State.Add("GameSaveState", state);

        //    base.OnDeactivated(sender, args);
        //}


        //#endregion
        protected override void OnExiting(object sender, System.EventArgs args)
        {

            base.OnExiting(sender, args);
        }

    }
}
