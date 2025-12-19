#region File Description
//-----------------------------------------------------------------------------
// ShipGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using System;
#endregion

namespace ShipGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ShipGameGame : Microsoft.Xna.Framework.Game
    {
        static ShipGameGame instance;

        GraphicsDeviceManager graphics;
        ScreenManager screen;
        GameManager game;
        FontManager font;
        SoundManager soundManager;

        bool renderVsync = true;

        public ShipGameGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.Title = "ShipGame";

            soundManager = new SoundManager();
            game = new GameManager(soundManager);

            // On desktop, use the preferred resolution from GameOptions
            // On mobile/consoles, these values will be overridden by the device's native resolution
#if WINDOWS || WINDOWS_UAP || DESKTOPGL
            graphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
            graphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;
#else
            // On mobile platforms, use native resolution
            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#endif

            IsFixedTimeStep = renderVsync;
            graphics.SynchronizeWithVerticalRetrace = renderVsync;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to 
        /// run. This is where it can query for any required services and load any 
        /// non-graphic related content. Calling base.Initialize will enumerate through 
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Update GameOptions with actual screen dimensions after device is created
            // This ensures resolution-dependent code uses the correct values on all platforms
            GameOptions.ScreenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            GameOptions.ScreenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            // Scale glow resolution proportionally for lower-res devices
            // Keep at 512 for devices with width >= 1280, scale down for smaller screens
            if (GameOptions.ScreenWidth < 1280)
            {
                GameOptions.GlowResolution = (int)(512 * (GameOptions.ScreenWidth / 1280.0f));
                // Ensure minimum glow resolution of 256 for quality
                GameOptions.GlowResolution = Math.Max(256, GameOptions.GlowResolution);
            }
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            font = new FontManager(graphics.GraphicsDevice);
            screen = new ScreenManager(this, font, game);

            soundManager.LoadContent(Content);
            font.LoadContent(Content);
            game.LoadContent(graphics.GraphicsDevice, Content);
            screen.LoadContent(graphics.GraphicsDevice, Content);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            soundManager.UnloadContent();
            screen.UnloadContent();
            game.UnloadContent();
            font.UnloadContent();

            screen = null;
            font = null;
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float ElapsedTimeFloat = (float)gameTime.ElapsedGameTime.TotalSeconds;

            screen.ProcessInput(ElapsedTimeFloat);
            screen.Update(ElapsedTimeFloat);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            screen.Draw(graphics.GraphicsDevice);

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called to switch full screen mode.
        /// </summary>
        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen();
        }

        static public ShipGameGame GetInstance()
        {
            return instance;
        }

        static public void SetInstance(ShipGameGame game)
        {
            instance = game;
        }
    }
}