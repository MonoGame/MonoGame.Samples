using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    //Pong that plays itself, with soundfx and a musical jingle
    //by MrGrak circa 2022, mit license

    public static class AutoPong
    {
        public static GraphicsDeviceManager GDM;
        public static ContentManager CM;
        public static SpriteBatch SB;
        public static Game1 GAME;

        public static Point GameBounds = new Point(1280, 720); //window resolution

        public static Rectangle PaddleLeft;
        public static Rectangle PaddleRight;

        public static Rectangle Ball;
        public static Vector2 BallVelocity;
        public static Vector2 BallPosition;
        public static float BallSpeed = 15.0f;

        public static Texture2D Texture;
        public static Rectangle DrawRec = new Rectangle(0, 0, 3, 3);

        static Random Rand = new Random();
        public static byte HitCounter = 0;
        
        public static int PointsLeft;
        public static int PointsRight;
        public static int PointsPerGame = 4;

        public static AudioSource SoundFX;
        public static int JingleCounter = 0;

        public static void Reset()
        {
            if (Texture == null)
            {   //create texture to draw with if it doesn't exist
                Texture = new Texture2D(GDM.GraphicsDevice, 1, 1);
                Texture.SetData<Color>(new Color[] { Color.White });
            }

            int PaddleHeight = 100;
            PaddleLeft = new Rectangle(0 + 10, 150, 20, PaddleHeight);
            PaddleRight = new Rectangle(GameBounds.X - 30, 150, 20, PaddleHeight);

            BallPosition = new Vector2(GameBounds.X / 2, 200);
            Ball = new Rectangle((int)BallPosition.X, (int)BallPosition.Y, 10, 10);
            BallVelocity = new Vector2(1, 0.1f);

            PointsLeft = 0; PointsRight = 0;
            JingleCounter = 0;

            //setup sound sources
            if (SoundFX == null) { SoundFX = new AudioSource(); }
        }

        public static void Update()
        {

            #region Update Ball

            //limit how fast ball can move each frame
            float maxVelocity = 1.5f;
            if (BallVelocity.X > maxVelocity) { BallVelocity.X = maxVelocity; }
            else if (BallVelocity.X < -maxVelocity) { BallVelocity.X = -maxVelocity; }
            if (BallVelocity.Y > maxVelocity) { BallVelocity.Y = maxVelocity; }
            else if (BallVelocity.Y < -maxVelocity) { BallVelocity.Y = -maxVelocity; }

            //apply velocity to position
            BallPosition.X += BallVelocity.X * BallSpeed;
            BallPosition.Y += BallVelocity.Y * BallSpeed;

            //check for collision with paddles
            HitCounter++;
            if (HitCounter > 10)
            {
                if (PaddleLeft.Intersects(Ball))
                {
                    BallVelocity.X *= -1;
                    BallVelocity.Y *= 1.1f;
                    HitCounter = 0;
                    BallPosition.X = PaddleLeft.X + PaddleLeft.Width + 10;
                    PlayWave(220.0f, 50, WaveType.Sin, SoundFX, 0.3f);
                }
                if (PaddleRight.Intersects(Ball))
                {
                    BallVelocity.X *= -1;
                    BallVelocity.Y *= 1.1f;
                    HitCounter = 0;
                    BallPosition.X = PaddleRight.X - 10;
                    PlayWave(220.0f, 50, WaveType.Sin, SoundFX, 0.3f);
                }
            }

            //bounce on screen
            if (BallPosition.X < 0) //point for right
            {
                BallPosition.X = 1;
                BallVelocity.X *= -1;
                PointsRight++;
                PlayWave(440.0f, 50, WaveType.Square, SoundFX, 0.3f);
            }
            else if (BallPosition.X > GameBounds.X) //point for left
            {
                BallPosition.X = GameBounds.X - 1;
                BallVelocity.X *= -1;
                PointsLeft++;
                PlayWave(440.0f, 50, WaveType.Square, SoundFX, 0.3f);
            }

            if (BallPosition.Y < 0 + 10) //limit to minimum Y pos
            {
                BallPosition.Y = 10 + 1;
                BallVelocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
            }
            else if (BallPosition.Y > GameBounds.Y - 10) //limit to maximum Y pos
            {
                BallPosition.Y = GameBounds.Y - 11;
                BallVelocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
            }

            #endregion

            #region Simulate Left Paddle Input

            {   //simple ai, not very good, moves random amount each frame
                int amount = Rand.Next(0, 6);
                int Paddle_Center = PaddleLeft.Y + PaddleLeft.Height / 2;
                if (Paddle_Center < BallPosition.Y - 20) { PaddleLeft.Y += amount; }
                else if (Paddle_Center > BallPosition.Y + 20) { PaddleLeft.Y -= amount; }
                LimitPaddle(ref PaddleLeft);
            }

            #endregion

            #region Simulate Right Paddle Input

            {   //simple ai, better than left, moves % each frame
                int Paddle_Center = PaddleRight.Y + PaddleRight.Height / 2;
                if (Paddle_Center < BallPosition.Y - 20) 
                { PaddleRight.Y -= (int)((Paddle_Center - BallPosition.Y) * 0.08f); }
                else if (Paddle_Center > BallPosition.Y + 20) 
                { PaddleRight.Y += (int)((BallPosition.Y - Paddle_Center) * 0.08f); }
                LimitPaddle(ref PaddleRight);
            }

            #endregion

            //Check for win condition, reset
            if (PointsLeft >= PointsPerGame) { Reset(); }
            else if (PointsRight >= PointsPerGame) { Reset(); }

            #region Play Reset Jingle

            //use jingle counter as a timeline to play notes
            JingleCounter++;

            int speed = 7;
            if (JingleCounter == speed * 1) { PlayWave(440.0f, 100, WaveType.Sin, SoundFX, 0.2f); }
            else if (JingleCounter == speed * 2) { PlayWave(523.25f, 100, WaveType.Sin, SoundFX, 0.2f); }
            else if (JingleCounter == speed * 3) { PlayWave(659.25f, 100, WaveType.Sin, SoundFX, 0.2f); }
            else if (JingleCounter == speed * 4) { PlayWave(783.99f, 100, WaveType.Sin, SoundFX, 0.2f); }
            //only play this jingle once
            else if (JingleCounter > speed * 4) { JingleCounter = int.MaxValue - 1; } 

            #endregion
        
        }

        public static void Draw()
        {
            SB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            //draw dots down center
            int total = GameBounds.Y / 20;
            for(int i = 0; i < total; i++)
            { DrawRectangle(new Rectangle(GameBounds.X / 2 - 4, 5 + (i * 20), 8, 8), Color.White * 0.2f); }

            //draw paddles
            DrawRectangle(PaddleLeft, Color.White);
            DrawRectangle(PaddleRight, Color.White);

            //draw ball
            Ball.X = (int)BallPosition.X;
            Ball.Y = (int)BallPosition.Y;
            DrawRectangle(Ball, Color.White);

            //draw current game points
            for (int i = 0; i < PointsLeft; i++)
            { DrawRectangle(new Rectangle((GameBounds.X / 2 - 25) - i * 12, 10, 10, 10), Color.White * 1.0f); }
            for (int i = 0; i < PointsRight; i++)
            { DrawRectangle(new Rectangle((GameBounds.X / 2 + 15) + i * 12, 10, 10, 10), Color.White * 1.0f); }

            SB.End();
        }

        public static void DrawRectangle(Rectangle Rec, Color color)
        {
            Vector2 pos = new Vector2(Rec.X, Rec.Y);
            SB.Draw(Texture, pos, Rec,
                color * 1.0f,
                0, Vector2.Zero, 1.0f,
                SpriteEffects.None, 0.00001f);
        }

        public static void LimitPaddle(ref Rectangle Paddle)
        {
            //limit how far paddles can travel on Y axis so they dont exceed top or bottom
            if (Paddle.Y < 10) { Paddle.Y = 10; }
            else if (Paddle.Y + Paddle.Height > GameBounds.Y - 10) 
            { Paddle.Y = GameBounds.Y - 10 - Paddle.Height; }
        }

        public static void PlayWave(double freq, short durMS, WaveType Wt, AudioSource Src, float Volume)
        {
            Src.DSEI.Stop();

            Src.BufferSize = Src.DSEI.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(durMS));
            Src.Buffer = new byte[Src.BufferSize];

            int size = Src.BufferSize - 1;
            for (int i = 0; i < size; i += 2)
            {
                double time = (double)Src.TotalTime / (double)Src.SampleRate;

                short currentSample = 0;
                switch (Wt)
                {
                    case WaveType.Sin:
                        {
                            currentSample = (short)(Math.Sin(2 * Math.PI * freq * time) * (double)short.MaxValue * Volume);
                            break;
                        }
                    case WaveType.Tan:
                        {
                            currentSample = (short)(Math.Tan(2 * Math.PI * freq * time) * (double)short.MaxValue * Volume);
                            break;
                        }
                    case WaveType.Square:
                        {
                            currentSample = (short)(Math.Sign(Math.Sin(2 * Math.PI * freq * time)) * (double)short.MaxValue * Volume);
                            break;
                        }
                    case WaveType.Noise:
                        {
                            currentSample = (short)(Rand.Next(-short.MaxValue, short.MaxValue) * Volume);
                            break;
                        }
                }

                Src.Buffer[i] = (byte)(currentSample & 0xFF);
                Src.Buffer[i + 1] = (byte)(currentSample >> 8);
                Src.TotalTime += 2;
            }

            Src.DSEI.SubmitBuffer(Src.Buffer);
            Src.DSEI.Play();
        }
    
    }

    public enum WaveType { Sin, Tan, Square, Noise }

    public class AudioSource
    {
        public int SampleRate = 48000;
        public DynamicSoundEffectInstance DSEI;
        public byte[] Buffer;
        public int BufferSize;
        public int TotalTime = 0;

        public AudioSource()
        {
            DSEI = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
            BufferSize = DSEI.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500));
            Buffer = new byte[BufferSize];
            DSEI.Volume = 0.4f;
            DSEI.IsLooped = false;
        }
    }

    public class Game1 : Game
    {
        public Game1()
        {
            AutoPong.GDM = new GraphicsDeviceManager(this);
            AutoPong.GDM.GraphicsProfile = GraphicsProfile.HiDef;
            AutoPong.CM = Content;
            AutoPong.GAME = this;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            AutoPong.GDM.PreferredBackBufferWidth = AutoPong.GameBounds.X;
            AutoPong.GDM.PreferredBackBufferHeight = AutoPong.GameBounds.Y;
        }

        protected override void Initialize() { base.Initialize(); }

        protected override void LoadContent()
        {
            AutoPong.SB = new SpriteBatch(GraphicsDevice);
            AutoPong.Reset();
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            AutoPong.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            AutoPong.Draw();
            base.Draw(gameTime);
        }
    }

}