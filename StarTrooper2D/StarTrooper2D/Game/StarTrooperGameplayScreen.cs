#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GameStateManagement;
#endregion

namespace StarTrooper2D
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class StarTrooper2DplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        public static SpriteFont font;

        public static Texture2D BlankTexture;


        TimeSpan SpawnElapsedTime;
        TimeSpan CondorSpawnRate = TimeSpan.FromSeconds(5);

        public static TimerDisplay PerformanceTimer;

        public long m_LastBaseDrawTime;



        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public StarTrooper2DplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                // TODO: use this.Content to load your game content here
                GameConstants.ParticleManager = new ParticleManager(ScreenManager.Game, ScreenManager.SpriteBatch);
                ScreenManager.Game.Components.Add(GameConstants.ParticleManager);

                LoadResources();

                AudioEngine.LoadContent(content);
                AudioEngine.PlayMusic();


                //Try and load any saved key mappings
                //FileManager.LoadKeyMappings();

                //If no settings present or setting were unable to be loaded, use the defaults
                if (!Input.InputMappings.SettingsSaved) Input.Load_Defaults();



                font = content.Load<SpriteFont>(@"Fonts\SpriteFont1");

                GameConstants.ScoreText = new Text2D(font);
                GameConstants.ScoreText.Text = "Score: " + GameState.score.ToString();
                GameConstants.ScoreText.Position = new Vector2(10, 10);
                GameConstants.ScoreText.Color = Color.Red;
                GameFunctions.Add(GameConstants.ScoreText);

                GameConstants.ShotsText = new Text2D(font);
                GameConstants.ShotsText.Text = "Shots: " + GameState.shots.ToString();
                GameConstants.ShotsText.Position = new Vector2(150, 10);
                GameConstants.ShotsText.Color = Color.Red;
                GameFunctions.Add(GameConstants.ShotsText);

                BlankTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                BlankTexture.SetData(new Color[] { Color.White });


                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            //content.Unload();
            AudioEngine.StopMusic();
            //Important or the Accelerometer will keep on running!!
            Input.Dispose();

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                // TODO: Add your update logic here
                InternalUpdate(); // <- Added Sprite update function here

                SpawnElapsedTime += gameTime.ElapsedGameTime;
                if (SpawnElapsedTime > CondorSpawnRate)
                {
                    Condor condor = (Condor)GameConstants.Condor.Clone();
                    condor.Position = new Vector2(GameFunctions.Random.Next(-100, (int)GameConstants.BackBufferWidth + 100), -150);
                    GameState.m_AddedSprites.Add(condor);
                    SpawnElapsedTime = TimeSpan.Zero;
                }
                GameConstants.ScoreText.Text = "Score: " + GameState.score.ToString();
                GameConstants.ShotsText.Text = "Shots: " + GameState.shots.ToString();
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

             // if the user pressed the back button, we return to the main menu
            PlayerIndex player;
            if (input.IsMenuCancel(ControllingPlayer, out player))
            {
                LoadingScreen.Load(ScreenManager, false, ControllingPlayer, new BackgroundScreen(), new MainMenuScreen());
            }
            else
            {
                GameState.Trooper.HandleInput(gameTime,input,ControllingPlayer);
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            GameState.m_ZOrderedSprites.Sort(Sprite.ComparisonZOrder);// <- Added Sprite draw code here

            foreach (Sprite s in GameState.m_ZOrderedSprites)
                s.Draw(gameTime, spriteBatch);

            foreach (Text2D t in GameState.m_Text2Ds)
                t.Draw(gameTime, spriteBatch);

            
            
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }


        #endregion

        #region Helper Functions


        void InternalUpdate()
        {
            foreach (Sprite s in GameState.m_AddedSprites)
            {
                GameState.m_Sprites.Add(s);
                GameState.m_ZOrderedSprites.Add(s);
            }
            GameState.m_AddedSprites.Clear();

            foreach (Sprite s in GameState.m_DeletedSprites)
            {
                GameState.m_Sprites.Remove(s);
                GameState.m_ZOrderedSprites.Remove(s);
            }
            GameState.m_DeletedSprites.Clear();

            foreach (Sprite s in GameState.m_Sprites)
                s.InternalUpdate();

            foreach (Sprite s in GameState.m_Sprites)
                s.Update();


            foreach (Text2D t in GameState.m_AddedText2Ds)
                GameState.m_Text2Ds.Add(t);
            GameState.m_AddedText2Ds.Clear();

            foreach (Text2D t in GameState.m_DeletedText2Ds)
                GameState.m_Text2Ds.Remove(t);
            GameState.m_DeletedText2Ds.Clear();

            foreach (Text2D t in GameState.m_Text2Ds)
                t.InternalUpdate();

        }

        public void LoadResources()
        {

            #region Background
            //Type the code here to add the background to the game.

            Texture2D background = content.Load<Texture2D>(@"Pictures\background");

            Background bg = new Background(background);

            bg.Location = new Vector2(0, GameConstants.BackBufferHeight / 2);
            bg.ScaleX = GameConstants.BackBufferWidth / background.Width;
            bg.ScaleY = GameConstants.BackBufferHeight / background.Height;
            bg.ZOrder = 10;
            bg.Origin = Vector2.Zero;
            bg.Velocity = new Vector2(0, 1);

            GameState.m_AddedSprites.Add(bg);

            Background bg2 = (Background)bg.Clone();
            bg2.Location = new Vector2(0, -GameConstants.BackBufferHeight / 2);
            bg2.ScaleX = GameConstants.BackBufferWidth / background.Width;
            bg2.ScaleY = GameConstants.BackBufferHeight / background.Height;
            bg2.ZOrder = 10;

            GameState.m_AddedSprites.Add(bg2);

            #endregion

            #region Trooper
            //Type the code here to add the Trooper sprite.
            Trooper trooper = new Trooper(content.Load<Texture2D>(@"Pictures\TrooperSpritesheet"), 6, true);

            trooper.Position = new Vector2(GameConstants.BackBufferWidth / 2, GameConstants.BackBufferHeight - (GameConstants.ScreenBuffer * 3));
            trooper.Speed = 4;
            GameState.m_AddedSprites.Add(trooper);

            GameState.Trooper = trooper;
            #endregion

            #region Condor
            //Type the code here to add the Condor sprite.
            Condor condor = new Condor();

            Animation condorAnimation = new Animation(content.Load<Texture2D>(@"Pictures\CondorSpritesheet"), 4);

            condorAnimation.Play();
            condorAnimation.Loop = true;

            int[] ExplosionDelay = { 4, 3, 4 };
            Animation condorExplosion = new Animation(content.Load<Texture2D>(@"Pictures\CondorExplosionSpritesheet"), 3, ExplosionDelay);

            condorExplosion.Play();

            condor.AddAnimation(condorAnimation);
            condor.AddAnimation(condorExplosion);
            GameConstants.Condor = condor;

            #endregion

            #region Fire
            GameConstants.Fire = new Fire(content.Load<Texture2D>(@"Pictures\FireSpritesheet"), 2, true);
            GameConstants.Fire.ZOrder = -10;
            #endregion

        }
        #endregion

    }
}
