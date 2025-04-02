using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AutoPong
{
    public class AutoPongGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Point GameBounds = new Point(1280, 720); //window resolution

        private Rectangle PaddleLeft;
        private Rectangle PaddleRight;

        private Rectangle Ball;
        private Vector2 BallVelocity;
        private Vector2 BallPosition;
        private float BallSpeed = 15.0f;

        public Texture2D Texture;

        private Random Rand = new Random();
        private byte HitCounter = 0;

        private int PointsLeft;
        private int PointsRight;
        private int PointsPerGame = 4;

        private AudioSource SoundFX;
        private int JingleCounter = 0;

        public AutoPongGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Reset();
        }

        protected override void Update(GameTime gameTime)
        {
            if (!OperatingSystem.IsIOS())
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
            }

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
                    SoundFX.PlayWave(220.0f, 50, WaveType.Sin, 0.3f);
                }
                if (PaddleRight.Intersects(Ball))
                {
                    BallVelocity.X *= -1;
                    BallVelocity.Y *= 1.1f;
                    HitCounter = 0;
                    BallPosition.X = PaddleRight.X - 10;
                    SoundFX.PlayWave(220.0f, 50, WaveType.Sin, 0.3f);
                }
            }

            //bounce on screen
            if (BallPosition.X < 0) //point for right
            {
                BallPosition.X = 1;
                BallVelocity.X *= -1;
                PointsRight++;
                SoundFX.PlayWave(440.0f, 50, WaveType.Square, 0.3f);
            }
            else if (BallPosition.X > GameBounds.X) //point for left
            {
                BallPosition.X = GameBounds.X - 1;
                BallVelocity.X *= -1;
                PointsLeft++;
                SoundFX.PlayWave(440.0f, 50, WaveType.Square, 0.3f);
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
            #endregion Simulate Left Paddle Input

            #region Simulate Right Paddle Input
            {   //simple ai, better than left, moves % each frame
                int Paddle_Center = PaddleRight.Y + PaddleRight.Height / 2;
                if (Paddle_Center < BallPosition.Y - 20)
                { PaddleRight.Y -= (int)((Paddle_Center - BallPosition.Y) * 0.08f); }
                else if (Paddle_Center > BallPosition.Y + 20)
                { PaddleRight.Y += (int)((BallPosition.Y - Paddle_Center) * 0.08f); }
                LimitPaddle(ref PaddleRight);
            }
            #endregion Simulate Right Paddle Input

            #region Check Win
            //Check for win condition, reset
            if (PointsLeft >= PointsPerGame) { Reset(); }
            else if (PointsRight >= PointsPerGame) { Reset(); }
            #endregion Check Win

            #region Play Reset Jingle
            //use jingle counter as a timeline to play notes
            JingleCounter++;

            int speed = 7;
            if (JingleCounter == speed * 1) { SoundFX.PlayWave(440.0f, 100, WaveType.Sin, 0.2f); }
            else if (JingleCounter == speed * 2) { SoundFX.PlayWave(523.25f, 100, WaveType.Sin, 0.2f); }
            else if (JingleCounter == speed * 3) { SoundFX.PlayWave(659.25f, 100, WaveType.Sin, 0.2f); }
            else if (JingleCounter == speed * 4) { SoundFX.PlayWave(783.99f, 100, WaveType.Sin, 0.2f); }
            //only play this jingle once
            else if (JingleCounter > speed * 4) { JingleCounter = int.MaxValue - 1; }
            #endregion Play Reset Jingle

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            //draw dots down center
            int total = GameBounds.Y / 20;
            for (int i = 0; i < total; i++)
            {
                DrawRectangle(_spriteBatch, new Rectangle(GameBounds.X / 2 - 4, 5 + (i * 20), 8, 8), Color.White * 0.2f);
            }

            //draw paddles
            DrawRectangle(_spriteBatch, PaddleLeft, Color.White);
            DrawRectangle(_spriteBatch, PaddleRight, Color.White);

            //draw ball
            Ball.X = (int)BallPosition.X;
            Ball.Y = (int)BallPosition.Y;
            DrawRectangle(_spriteBatch, Ball, Color.White);

            //draw current game points
            for (int i = 0; i < PointsLeft; i++)
            {
                DrawRectangle(_spriteBatch, new Rectangle((GameBounds.X / 2 - 25) - i * 12, 10, 10, 10), Color.White * 1.0f);
            }
            for (int i = 0; i < PointsRight; i++)
            {
                DrawRectangle(_spriteBatch, new Rectangle((GameBounds.X / 2 + 15) + i * 12, 10, 10, 10), Color.White * 1.0f);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawRectangle(SpriteBatch sb, Rectangle Rec, Color color)
        {
            Vector2 pos = new Vector2(Rec.X, Rec.Y);
            sb.Draw(Texture, pos, Rec,
                color * 1.0f,
                0, Vector2.Zero, 1.0f,
                SpriteEffects.None, 0.00001f);
        }

        private void LimitPaddle(ref Rectangle Paddle)
        {
            //limit how far paddles can travel on Y axis so they dont exceed top or bottom
            if (Paddle.Y < 10) { Paddle.Y = 10; }
            else if (Paddle.Y + Paddle.Height > GameBounds.Y - 10)
            { Paddle.Y = GameBounds.Y - 10 - Paddle.Height; }
        }

        private void Reset()
        {
            if (Texture == null)
            {   //create texture to draw with if it does not exist
                Texture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
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
            if (SoundFX == null)
            {
                SoundFX = new AudioSource();
            }
        }
    }
}