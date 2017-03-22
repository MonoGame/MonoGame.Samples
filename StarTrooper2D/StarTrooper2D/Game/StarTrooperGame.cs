using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StarTrooper2D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class StarTrooperGame : Microsoft.Xna.Framework.Game
    {
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;

        //Game management related properties
        static List<Sprite> m_Sprites = new List<Sprite>();
        static List<Sprite> m_ZOrderedSprites = new List<Sprite>();
        static List<Sprite> m_DeletedSprites = new List<Sprite>();
        static List<Sprite> m_AddedSprites = new List<Sprite>();

        static List<Text2D> m_Text2Ds = new List<Text2D>();
        static List<Text2D> m_DeletedText2Ds = new List<Text2D>();
        static List<Text2D> m_AddedText2Ds = new List<Text2D>();


        private static int m_TargetFrameRate = 30;
        private const int m_BackBufferWidth = 480;
        private const int m_BackBufferHeight = 800;
        private const int m_ScreenBuffer = 10;

        public static Trooper Trooper;
        public static Condor Condor;
        public static Fire Fire;

        public static SoundEffect Shoot;
        public static SoundEffect Die;
        public static SoundEffect Music;
        public static SoundEffectInstance BackgroundMusic;

        public static SpriteFont font;
        public static int score;
        public static Text2D ScoreText;

        public static int shots;
        public static Text2D ShotsText;

        public static Texture2D BlankTexture;


        static Random m_Random = new Random();
        TimeSpan SpawnElapsedTime;
        TimeSpan CondorSpawnRate = TimeSpan.FromSeconds(5);

        public static ParticleManager ParticleManager;

        public static TimerDisplay PerformanceTimer;

        public long m_LastBaseDrawTime;

        public StarTrooperGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            ParticleManager = new ParticleManager(this,spriteBatch);
            this.Components.Add(ParticleManager);

            // Pre-autoscale settings.
            graphics.PreferredBackBufferWidth = m_BackBufferWidth;
            graphics.PreferredBackBufferHeight = m_BackBufferHeight;

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

            base.Initialize();
#if TESTPERFORMANCE
            PerformanceTimer = new TimerDisplay(this);
            Components.Add(PerformanceTimer);

            PerformanceTimer.SetupTimer("Time to update in ticks: ");
            PerformanceTimer.SetupTimer("Time to draw game in ticks: ");
            PerformanceTimer.SetupTimer("Time to Sort Sprites: ");
            PerformanceTimer.SetupTimer("Time to draw Sprites: ");
            PerformanceTimer.SetupTimer("Time to draw Text: ");
            PerformanceTimer.SetupTimer("Time to draw Base: ");
            PerformanceTimer.SetupTimer("Time to draw full particle manager in ticks: ");
            PerformanceTimer.SetupTimer("Time to draw emitter particles in ticks: ");
            PerformanceTimer.SetupTimer("Performance Draw");
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            LoadResources();

            //Try and load any saved key mappings
            //FileManager.LoadKeyMappings();

            //If no settings present or setting were unable to be loaded, use the defaults
            if (!Input.InputMappings.SettingsSaved) Input.Load_Defaults();

            Shoot = Content.Load<SoundEffect>(@"Sounds\shoot");
            Die = Content.Load<SoundEffect>(@"Sounds\die");


            Music = Content.Load<SoundEffect>(@"Music\music");
            BackgroundMusic = Music.CreateInstance();
            BackgroundMusic.IsLooped = true;

            BackgroundMusic.Play();

            font = Content.Load<SpriteFont>(@"Fonts\SpriteFont1");

            ScoreText = new Text2D(font);
            ScoreText.Text = "Score: " + score.ToString();
            ScoreText.Position = new Vector2(10, 10);
            ScoreText.Color = Color.Red;
            StarTrooperGame.Add(ScoreText);

            ShotsText = new Text2D(font);
            ShotsText.Text = "Shots: " + shots.ToString();
            ShotsText.Position = new Vector2(150, 10);
            ShotsText.Color = Color.Red;
            StarTrooperGame.Add(ShotsText);

            BlankTexture = new Texture2D(GraphicsDevice, 1, 1);
            BlankTexture.SetData(new Color[] { Color.White });
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Dispose();
            //Important or the Accelerometer will keep on running!!
            Input.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to update in ticks: ");
#endif
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //{
            //    FileManager.SaveKeyMappings();
            //    this.Exit();
            //}
            Input.Update();
            // TODO: Add your update logic here
            InternalUpdate(); // <- Added Sprite update function here

            SpawnElapsedTime += gameTime.ElapsedGameTime;
            if (SpawnElapsedTime > CondorSpawnRate)
            {
                Condor condor = (Condor)Condor.Clone();
                condor.Position = new Vector2(m_Random.Next(-100, m_BackBufferWidth + 100), -150);
                m_AddedSprites.Add(condor);
                SpawnElapsedTime = TimeSpan.Zero;
            }
            ScoreText.Text = "Score: " + score.ToString();
            ShotsText.Text = "Shots: " + shots.ToString();


            base.Update(gameTime);
#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to update in ticks: ");
#endif
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to draw game in ticks: ");
#endif
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to Sort Sprites: ");
#endif
            m_ZOrderedSprites.Sort(Sprite.ComparisonZOrder);// <- Added Sprite draw code here
#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to Sort Sprites: ");
#endif
#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to draw Sprites: ");
#endif
            foreach (Sprite s in m_ZOrderedSprites)
                s.Draw(gameTime, spriteBatch);
#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to draw Sprites: ");
#endif
#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to draw Text: ");
#endif
            foreach (Text2D t in m_Text2Ds)
                t.Draw(gameTime, spriteBatch);
#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to draw Text: ");
#endif

#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to draw game in ticks: ");
#endif

#if TESTPERFORMANCE
            PerformanceTimer.StartTimer("Time to draw Base: ",m_LastBaseDrawTime);
            m_LastBaseDrawTime = DateTime.Now.Ticks;
#endif
            base.Draw(gameTime);
#if TESTPERFORMANCE
            PerformanceTimer.StopTimer("Time to draw Base: ");
#endif
        }

        public static void Add(Sprite sprite)
        {
            m_AddedSprites.Add(sprite);
        }

        public static void Add(Text2D text)
        {
            m_AddedText2Ds.Add(text);
        }

        public static void Remove(Sprite sprite)
        {
            m_DeletedSprites.Add(sprite);
        }

        void InternalUpdate()
        {
            foreach (Sprite s in m_AddedSprites)
            {
                m_Sprites.Add(s);
                m_ZOrderedSprites.Add(s);
            }
            m_AddedSprites.Clear();

            foreach (Sprite s in m_DeletedSprites)
            {
                m_Sprites.Remove(s);
                m_ZOrderedSprites.Remove(s);
            }
            m_DeletedSprites.Clear();

            foreach (Sprite s in m_Sprites)
                s.InternalUpdate();

            foreach (Sprite s in m_Sprites)
                s.Update();


            foreach (Text2D t in m_AddedText2Ds)
                m_Text2Ds.Add(t);
            m_AddedText2Ds.Clear();

            foreach (Text2D t in m_DeletedText2Ds)
                m_Text2Ds.Remove(t);
            m_DeletedText2Ds.Clear();

            foreach (Text2D t in m_Text2Ds)
                t.InternalUpdate();

        }

        public void LoadResources()
        {

            #region Background
            //Type the code here to add the background to the game.

            Texture2D background = Content.Load<Texture2D>(@"Pictures\background");

            Background bg = new Background(background);

            bg.Location = new Vector2(0, BackBufferHeight / 2);
            bg.ScaleX = BackBufferWidth / background.Width;
            bg.ScaleY = BackBufferHeight / background.Height;
            bg.ZOrder = 10;
            bg.Origin = Vector2.Zero;
            bg.Velocity = new Vector2(0, 1);

            m_AddedSprites.Add(bg);

            Background bg2 = (Background)bg.Clone();
            bg2.Location = new Vector2(0, -BackBufferHeight / 2);
            bg2.ScaleX = BackBufferWidth / background.Width;
            bg2.ScaleY = BackBufferHeight / background.Height;
            bg2.ZOrder = 10;

            m_AddedSprites.Add(bg2);

            #endregion

            #region Trooper
            //Type the code here to add the Trooper sprite.
            Trooper trooper = new Trooper(Content.Load<Texture2D>(@"Pictures\TrooperSpritesheet"), 6, true);

            trooper.Position = new Vector2(BackBufferWidth / 2, BackBufferHeight - (ScreenBuffer * 3));
            trooper.Speed = 4;
            m_AddedSprites.Add(trooper);

            Trooper = trooper;
            #endregion

            #region Condor
            //Type the code here to add the Condor sprite.
            Condor condor = new Condor();

            Animation condorAnimation = new Animation(Content.Load<Texture2D>(@"Pictures\CondorSpritesheet"), 4);

            condorAnimation.Play();
            condorAnimation.Loop = true;

            int[] ExplosionDelay = { 4, 3, 4 };
            Animation condorExplosion = new Animation(Content.Load<Texture2D>(@"Pictures\CondorExplosionSpritesheet"), 3, ExplosionDelay);

            condorExplosion.Play();

            condor.AddAnimation(condorAnimation);
            condor.AddAnimation(condorExplosion);
            Condor = condor;

            #endregion

            #region Fire
            Fire = new Fire(Content.Load<Texture2D>(@"Pictures\FireSpritesheet"), 2, true);
            Fire.ZOrder = -10;
            #endregion

        }

        #region Helper Functions

        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)m_Random.NextDouble() * (max - min);
        }

        public static List<Sprite> GetCollidedSprites(Sprite sprite)
        {
            List<Sprite> collisionList = new List<Sprite>();
            foreach (Sprite s in m_Sprites)
            {
                if (s != sprite && s.CollidesWith(sprite))
                    collisionList.Add(s);
            }

            if (collisionList.Count != 0)
                return collisionList;
            return null;
        }

        #endregion

        #region Properties
        public static float BackBufferWidth { get { return (float)m_BackBufferWidth; } }
        public static float BackBufferHeight { get { return (float)m_BackBufferHeight; } }
        public static float TargetFrameRate { set { m_TargetFrameRate = (int)value; } get { return (float)1f / m_TargetFrameRate; } }
        public static int ScreenBuffer { get { return (int)m_ScreenBuffer; } }
        // a random number generator that the whole sample can share.
        public static Random Random { get { return m_Random; } }
        #endregion
    }
}
