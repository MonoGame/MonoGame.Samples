#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields

        public Game game { get; set; }
        // Constants.
        const float TransitionSpeed = 1.5f;

        private const string StateFilename = "ScreenManagerState.xml";

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> tempScreensList = new List<GameScreen>();

        InputState input = new InputState();

        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D blankTexture;

        bool isInitialized;

        bool traceEnabled;

        // Transition effects provide swooshy crossfades when moving from one screen to another.
        float transitionTimer = float.MaxValue;
        int transitionMode = 0;

        RenderTarget2D transitionRenderTarget;

        GameTime currentGameTime;

        #endregion

        #region Properties


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }


        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        /// <summary>
        /// Gets a blank texture that can be used by the screens.
        /// </summary>
        public Texture2D BlankTexture
        {
            get { return blankTexture; }
        }


        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game)
            : base(game)
        {
            this.game = game;
            // we must set EnabledGestures before we can query for them, but
            // we don't assume the game wants to read them.
            TouchPanel.EnabledGestures = GestureType.None;
        }


        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            isInitialized = true;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = content.Load<SpriteFont>("MenuAssets/menufont");

            blankTexture = new Texture2D(GraphicsDevice, 1, 1);
            blankTexture.SetData(new Color[] { Color.White });

            transitionRenderTarget = new RenderTarget2D(GraphicsDevice, 480, 800, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, 0);

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.Activate(false);
            }
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.Unload();
            }
        }


        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            tempScreensList.Clear();

            foreach (GameScreen screen in screens)
                tempScreensList.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (tempScreensList.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = tempScreensList[tempScreensList.Count - 1];

                tempScreensList.RemoveAt(tempScreensList.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();

            // TODO: Add your update logic here
            currentGameTime = gameTime;


            if (transitionTimer < float.MaxValue)
                transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }

            DrawTransitionEffect();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (isInitialized)
            {
                screen.Activate(false);
            }

            screens.Add(screen);

            // update the TouchPanel to respond to gestures this screen is interested in
            TouchPanel.EnabledGestures = screen.EnabledGestures;
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (isInitialized)
            {
                screen.Unload();
            }

            screens.Remove(screen);
            tempScreensList.Remove(screen);

            // if there is a screen still in the manager, update TouchPanel
            // to respond to gestures that screen is interested in.
            if (screens.Count > 0)
            {
                TouchPanel.EnabledGestures = screens[screens.Count - 1].EnabledGestures;
            }
        }

        public String CurrentScreenName()
        {
            return screens[screens.Count - 1].GetType().Name;
        }

        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(BlankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.Black * alpha);

            spriteBatch.End();
        }

        /// <summary>
        /// Informs the screen manager to serialize its state to disk.
        /// </summary>
        public void Deactivate()
        {
#if !WINDOWS_PHONE
            return;
#else
            // Open up isolated storage
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create an XML document to hold the list of screen types currently in the stack
                XDocument doc = new XDocument();
                XElement root = new XElement("ScreenManager");
                doc.Add(root);

                // Make a copy of the master screen list, to avoid confusion if
                // the process of deactivating one screen adds or removes others.
                tempScreensList.Clear();
                foreach (GameScreen screen in screens)
                    tempScreensList.Add(screen);

                // Iterate the screens to store in our XML file and deactivate them
                foreach (GameScreen screen in tempScreensList)
                {
                    // Only add the screen to our XML if it is serializable
                    if (screen.IsSerializable)
                    {
                        // We store the screen's controlling player so we can rehydrate that value
                        string playerValue = screen.ControllingPlayer.HasValue
                            ? screen.ControllingPlayer.Value.ToString()
                            : "";

                        root.Add(new XElement(
                            "GameScreen",
                            new XAttribute("Type", screen.GetType().AssemblyQualifiedName),
                            new XAttribute("ControllingPlayer", playerValue)));
                    }

                    // Deactivate the screen regardless of whether we serialized it
                    screen.Deactivate();
                }

                // Save the document
                using (IsolatedStorageFileStream stream = storage.CreateFile(StateFilename))
                {
                    doc.Save(stream);
                }
            }
#endif
        }

        public bool Activate(bool instancePreserved)
        {
#if !WINDOWS_PHONE
            return false;
#else
            // If the game instance was preserved, the game wasn't dehydrated so our screens still exist.
            // We just need to activate them and we're ready to go.
            if (instancePreserved)
            {
                // Make a copy of the master screen list, to avoid confusion if
                // the process of activating one screen adds or removes others.
                tempScreensList.Clear();

                foreach (GameScreen screen in screens)
                    tempScreensList.Add(screen);

                foreach (GameScreen screen in tempScreensList)
                    screen.Activate(true);
            }

            // Otherwise we need to refer to our saved file and reconstruct the screens that were present
            // when the game was deactivated.
            else
            {
                // Try to get the screen factory from the services, which is required to recreate the screens
                IScreenFactory screenFactory = Game.Services.GetService(typeof(IScreenFactory)) as IScreenFactory;
                if (screenFactory == null)
                {
                    throw new InvalidOperationException(
                        "Game.Services must contain an IScreenFactory in order to activate the ScreenManager.");
                }

                // Open up isolated storage
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Check for the file; if it doesn't exist we can't restore state
                    if (!storage.FileExists(StateFilename))
                        return false;

                    // Read the state file so we can build up our screens
                    using (IsolatedStorageFileStream stream = storage.OpenFile(StateFilename, FileMode.Open))
                    {
                        XDocument doc = XDocument.Load(stream);

                        // Iterate the document to recreate the screen stack
                        foreach (XElement screenElem in doc.Root.Elements("GameScreen"))
                        {
                            // Use the factory to create the screen
                            Type screenType = Type.GetType(screenElem.Attribute("Type").Value);
                            GameScreen screen = screenFactory.CreateScreen(screenType);

                            // Rehydrate the controlling player for the screen
                            PlayerIndex? controllingPlayer = screenElem.Attribute("ControllingPlayer").Value != ""
                                ? (PlayerIndex)Enum.Parse(typeof(PlayerIndex), screenElem.Attribute("ControllingPlayer").Value, true)
                                : (PlayerIndex?)null;
                            screen.ControllingPlayer = controllingPlayer;

                            // Add the screen to the screens list and activate the screen
                            screen.ScreenManager = this;
                            screens.Add(screen);
                            screen.Activate(false);

                            // update the TouchPanel to respond to gestures this screen is interested in
                            TouchPanel.EnabledGestures = screen.EnabledGestures;
                        }
                    }
                }
            }

            return true;
#endif
        }

        //public void SerializeState()
        //{
        //    // open up isolated storage
        //    using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        // if our screen manager directory already exists
        //        if (!storage.DirectoryExists("ScreenManager"))
        //        {
        //            storage.CreateDirectory("ScreenManager");
        //        }

        //        // create a file we'll use to store the list of screens in the stack
        //        using (IsolatedStorageFileStream stream = storage.OpenFile("ScreenManager\\ScreenList.dat", FileMode.Create))
        //        {
        //            using (StreamWriter writer = new StreamWriter(stream))
        //            {
        //                // write out the full name of all the types in our stack so we can
        //                // recreate them if needed.
        //                foreach (GameScreen screen in screens)
        //                {
        //                    if (screen.IsSerializable)
        //                    {
        //                        writer.WriteLine(screen.GetType().AssemblyQualifiedName);
        //                    }
        //                }
        //            }
        //        }

        //        // now we create a new file stream for each screen so it can save its state
        //        // if it needs to. we name each file "ScreenX.dat" where X is the index of
        //        // the screen in the stack, to ensure the files are uniquely named
        //        int screenIndex = 0;
        //        foreach (GameScreen screen in screens)
        //        {
        //            if (screen.IsSerializable)
        //            {
        //                string fileName = string.Format("ScreenManager\\Screen{0}.dat", screenIndex);
        //                Debug.WriteLine("Serializing {0} to {1}", screen.GetType().Name, fileName);

        //                // open up the stream and let the screen serialize whatever state it wants
        //                using (IsolatedStorageFileStream stream = storage.CreateFile(fileName))
        //                {
        //                    screen.Serialize(stream);
        //                }

        //                screenIndex++;
        //            }
        //        }
        //    }
        //}

        //public bool DeserializeState()
        //{
        //    // open up isolated storage
        //    using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        // see if our saved state directory exists
        //        if (storage.DirectoryExists("ScreenManager"))
        //        {
        //            // see if we have a screen list
        //            if (storage.FileExists("ScreenManager\\ScreenList.dat"))
        //            {
        //                // load the list of screen types
        //                using (IsolatedStorageFileStream stream = storage.OpenFile("ScreenManager\\ScreenList.dat", FileMode.Open, FileAccess.Read))
        //                {
        //                    using (StreamReader reader = new StreamReader(stream))
        //                    {
        //                        while (!reader.EndOfStream)
        //                        {
        //                            // read a line from our file
        //                            string line = reader.ReadLine();

        //                            // if it isn't blank, we can create a screen from it
        //                            if (!string.IsNullOrEmpty(line))
        //                            {
        //                                Type screenType = Type.GetType(line);
        //                                GameScreen screen = Activator.CreateInstance(screenType) as GameScreen;
        //                                AddScreen(screen, PlayerIndex.One);
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            // next we give each screen a chance to deserialize from the disk
        //            for (int i = 0; i < screens.Count; i++)
        //            {
        //                string filename = string.Format("ScreenManager\\Screen{0}.dat", i);
        //                using (IsolatedStorageFileStream stream = storage.OpenFile(filename, FileMode.Open, FileAccess.Read))
        //                {
        //                    screens[i].Deserialize(stream);
        //                }
        //            }

        //            return true;
        //        }
        //    }

        //    return false;
        //}

        #endregion

        #region Transition Effects

        /// <summary>
        /// Begins a transition effect, capturing a copy of the current screen into the transitionRenderTarget.
        /// </summary>
        public void BeginTransition(int TransitionMode)
        {

            GraphicsDevice.SetRenderTarget(transitionRenderTarget);

            // Draw the old menu screen into the rendertarget.
            foreach (GameScreen screen in screens)
            {
                screen.Draw(currentGameTime);
            }

            // Force the rendertarget alpha channel to fully opaque.
            SpriteBatch.Begin(0, BlendState.Additive);
            SpriteBatch.Draw(BlankTexture, new Rectangle(0, 0, 480, 800), new Color(0, 0, 0, 255));
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            // Initialize the transition state.
            transitionTimer = (float)Game.TargetElapsedTime.TotalSeconds;
            transitionMode = TransitionMode;
        }

        /// <summary>
        /// Draws the transition effect, displaying various animating pieces of the rendertarget
        /// which contains the previous scene image over the top of the new scene. There are
        /// various different effects which animate these pieces in different ways.
        /// </summary>
        void DrawTransitionEffect()
        {
            if (transitionTimer >= TransitionSpeed)
                return;

            SpriteBatch.Begin();

            float mu = transitionTimer / TransitionSpeed;
            float alpha = 1 - mu;

            switch (transitionMode)
            {
                case 1:
                    // BasicEffect
                    DrawOpenCurtainsTransition(alpha);
                    break;

                case 2:
                    FadingWindowTransition(mu, alpha);
                    break;

                case 5:
                    // DualTexture
                    // EnvironmentMap
                    DrawSpinningSquaresTransition(mu, alpha);
                    break;

                case 3:
                case 4:
                    // AlphaTest and Skinning
                    DrawChequeredAppearTransition(mu);
                    break;

                case 6:
                    // Particles
                    DrawFallingLinesTransition(mu);
                    break;

                default:
                    // Returning to menu.
                    DrawShrinkAndSpinTransition(mu, alpha);
                    break;
            }

            SpriteBatch.End();
        }


        /// <summary>
        /// Transition effect where the screen splits in half, opening down the middle.
        /// </summary>
        void DrawOpenCurtainsTransition(float alpha)
        {
            int w = (int)(240 * alpha * alpha);

            SpriteBatch.Draw(transitionRenderTarget, new Rectangle(0, 0, w, 800), new Rectangle(0, 0, 240, 800), Color.White * alpha);
            SpriteBatch.Draw(transitionRenderTarget, new Rectangle(480 - w, 0, w, 800), new Rectangle(240, 0, 240, 800), Color.White * alpha);
        }


        /// <summary>
        /// Transition effect where the screen splits into pieces, each spinning off in a different direction.
        /// </summary>
        void DrawSpinningSquaresTransition(float mu, float alpha)
        {
            Random random = new Random(23);

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Rectangle rect = new Rectangle(480 * x / 4, 800 * y / 8, 480 / 4, 800 / 8);

                    Vector2 origin = new Vector2(rect.Width, rect.Height) / 2;

                    float rotation = (float)(random.NextDouble() - 0.5) * mu * mu * 2;
                    float scale = 1 + (float)(random.NextDouble() - 0.5f) * mu * mu;

                    Vector2 pos = new Vector2(rect.Center.X, rect.Center.Y);

                    pos.X += (float)(random.NextDouble() - 0.5) * mu * mu * 400;
                    pos.Y += (float)(random.NextDouble() - 0.5) * mu * mu * 400;

                    SpriteBatch.Draw(transitionRenderTarget, pos, rect, Color.White * alpha, rotation, origin, scale, 0, 0);
                }
            }
        }


        /// <summary>
        /// Transition effect where each square of the image appears at a different time.
        /// </summary>
        void DrawChequeredAppearTransition(float mu)
        {
            Random random = new Random(23);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    Rectangle rect = new Rectangle(480 * x / 8, 800 * y / 16, 480 / 8, 800 / 16);

                    if (random.NextDouble() > mu * mu)
                        SpriteBatch.Draw(transitionRenderTarget, rect, rect, Color.White);
                }
            }

        }


        /// <summary>
        /// Transition effect where the image dissolves into a sequence of vertically falling lines.
        /// </summary>
        void DrawFallingLinesTransition(float mu)
        {
            Random random = new Random(23);

            const int segments = 60;

            for (int x = 0; x < segments; x++)
            {
                Rectangle rect = new Rectangle(480 * x / segments, 0, 480 / segments, 800);

                Vector2 pos = new Vector2(rect.X, 0);

                pos.Y += 800 * (float)Math.Pow(mu, random.NextDouble() * 10);

                SpriteBatch.Draw(transitionRenderTarget, pos, rect, Color.White);
            }
        }


        /// <summary>
        /// Transition effect where the image spins off toward the bottom left of the screen.
        /// </summary>
        void DrawShrinkAndSpinTransition(float mu, float alpha)
        {
            Vector2 origin = new Vector2(240, 400);
            Vector2 translate = (new Vector2(32, 800 - 32) - origin) * mu * mu;

            float rotation = mu * mu * -4;
            float scale = alpha * alpha;

            Color tint = Color.White * (float)Math.Sqrt(alpha);

            SpriteBatch.Draw(transitionRenderTarget, origin + translate, null, tint, rotation, origin, scale, 0, 0);
        }

        /// <summary>
        /// Transition effect where the window just fades out.
        /// </summary>
        void FadingWindowTransition(float mu, float alpha)
        {
            Vector2 origin = new Vector2(0, 400);
            
            float rotation = mu * mu * -4;

            Color tint = Color.White * (float)Math.Sqrt(alpha);

            SpriteBatch.Draw(transitionRenderTarget, Vector2.Zero, tint);
        }


        #endregion

    }
}
