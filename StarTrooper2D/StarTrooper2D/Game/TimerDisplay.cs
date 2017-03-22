using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace StarTrooper2D
{
    public class Stopwatch
    {
        public long Start;
        public long Stop;
        public long Difference { get { return Stop - Start; } }
    }
    public class DisplayInfo
    {
        public String DisplayText;
        public int DisplayCount;
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TimerDisplay : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteFont font;
        SpriteBatch spritebatch;

        private Dictionary<String, Stopwatch> m_GameWatches = new Dictionary<String, Stopwatch>();
        private Dictionary<int, DisplayInfo> m_DisplayInformation = new Dictionary<int, DisplayInfo>();

        long m_LastPerformanceDrawTime;

        public TimerDisplay(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            font = Game.Content.Load<SpriteFont>("SpriteFont1");
            spritebatch = new SpriteBatch(game.GraphicsDevice);
            DrawOrder = 300;
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
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
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

               /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            StartTimer("Performance Draw", m_LastPerformanceDrawTime);
            m_LastPerformanceDrawTime = DateTime.Now.Ticks;
            int i = 10;
            spritebatch.Begin();
            foreach (String watch in m_GameWatches.Keys)
            {
                spritebatch.DrawString(font, watch + "-" + m_GameWatches[watch].Difference.ToString(), new Vector2(50, 55 + i), Color.White);
                i += 20;
            }
            foreach (int info in m_DisplayInformation.Keys)
            {
                spritebatch.DrawString(font, m_DisplayInformation[info].DisplayText + ": " + m_DisplayInformation[info].DisplayCount.ToString(), new Vector2(50, 55 + i), Color.White);
                i += 20;
            }

            spritebatch.End();
            StopTimer("Performance Draw");

            base.Draw(gameTime);
        }

        public void StartTimer(String StopwatchName, long Time)
        {
            m_GameWatches[StopwatchName].Start = Time;
        }

        public void StartTimer(String StopwatchName)
        {
            StartTimer(StopwatchName,DateTime.Now.Ticks);
        }

        public void StopTimer(String StopwatchName, long Time)
        {
            m_GameWatches[StopwatchName].Stop = Time;
        }

        public void StopTimer(String StopwatchName)
        {
            StopTimer(StopwatchName, DateTime.Now.Ticks);
        }

        public void SetupTimer(String StopwatchName)
        {
            Stopwatch Timer = new Stopwatch(); m_GameWatches.Add(StopwatchName, Timer);
        }

        public void AddUpdateDisplayInfo(int ID, String DisplayText,int value)
        {
            DisplayInfo info;
            try { info = m_DisplayInformation[ID]; }
            catch { info = new DisplayInfo(); m_DisplayInformation.Add(ID, info); }
            info.DisplayText = DisplayText;
            info.DisplayCount = value;
            m_DisplayInformation[ID] = info;

        }
        public void RemoveDisplayInfo(int ID)
        {
            try { m_DisplayInformation.Remove(ID); }
            catch { }
        }


    }

}