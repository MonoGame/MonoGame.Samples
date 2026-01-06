using System;
using System.Collections.Generic;
using System.Globalization;
using Platformer2D.Core.Effects;
using Platformer2D.Core.Localization;
using Platformer2D.Core.Settings;
using Platformer2D.ScreenManagers;
using Platformer2D.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    /// <remarks>
    /// This class is the entry point for the game and handles initialization, content loading,
    /// and screen management.
    /// </remarks>}
    public class Platformer2DGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;

        // Manages the game's screen transitions and screens.
        private ScreenManager screenManager;

        // Manages game settings, such as preferences and configurations.
        private SettingsManager<Platformer2DSettings> settingsManager;

        // Manages leaderboard data for tracking high scores and achievements.
        private SettingsManager<Platformer2DLeaderboard> leaderboardManager;

        // Texture for rendering particles.
        private Texture2D particleTexture;

        // Manages particle effects in the game.
        private ParticleManager particleManager;

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public Platformer2DGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            // Determine the appropriate settings storage based on the platform.
            ISettingsStorage storage;
            if (IsMobile)
            {
                storage = new MobileSettingsStorage();
                graphicsDeviceManager.IsFullScreen = true;
                IsMouseVisible = false;
            }
            else if (IsDesktop)
            {
                storage = new DesktopSettingsStorage();
                graphicsDeviceManager.IsFullScreen = false;
                graphicsDeviceManager.PreferredBackBufferWidth = 1280;
                graphicsDeviceManager.PreferredBackBufferHeight = 720;
                IsMouseVisible = true;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            // Initialize settings and leaderboard managers.
            settingsManager = new SettingsManager<Platformer2DSettings>(storage);
            Services.AddService(typeof(SettingsManager<Platformer2DSettings>), settingsManager);

            leaderboardManager = new SettingsManager<Platformer2DLeaderboard>(storage);
            Services.AddService(typeof(SettingsManager<Platformer2DLeaderboard>), leaderboardManager);

            Content.RootDirectory = "Content";

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // Initialize the screen manager.
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // Ensure the language index is valid, default to 0 if out of range
            int languageIndex = settingsManager.Settings.Language;
            if (languageIndex < 0 || languageIndex >= languages.Count)
            {
                languageIndex = 0;
                settingsManager.Settings.Language = 0;
                settingsManager.Save();
            }

            var selectedLanguage = languages[languageIndex].Name;
            LocalizationManager.SetCulture(selectedLanguage);

            // Add background and main menu screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Load a texture for particles and initialize the particle manager.
            particleTexture = Content.Load<Texture2D>("Sprites/blank");
            particleManager = new ParticleManager(particleTexture, new Vector2(400, 200));

            // Share the particle manager as a service.
            Services.AddService(typeof(ParticleManager), particleManager);
        }
    }
}