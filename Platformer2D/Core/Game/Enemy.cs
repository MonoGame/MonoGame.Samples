using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Core
{
    /// <summary>
    /// An enemy who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy
    {
        bool isAlive = true;
        /// <summary>
        /// Gets a value indicating whether the enemy is currently alive.
        /// </summary>
        public bool IsAlive => isAlive;

        Level level;
        /// <summary>
        /// Gets the level instance to which this enemy belongs.
        /// </summary>
        public Level Level
        {
            get { return level; }
        }

        Vector2 position;
        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 64.0f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        /// <param name="level">The level instance to which this enemy belongs.</param>
        /// <param name="position">The initial position of the enemy in world space.</param>
        /// <param name="spriteSet">The name of the sprite set to load for this enemy.</param>
        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        /// <param name="spriteSet">The name of the sprite set to load for this enemy.</param>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            sprite.PlayAnimation(idleAnimation);

            // Load sounds.
            killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            if (!isAlive)
                return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - elapsed);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else if (!Level.Paused)
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="spriteBatch">The SpriteBatch used to draw the enemy.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            // Stop running when the game is paused or before turning around.
            else if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                Level.TimeTaken == Level.MaximumTimeToCompleteLevel ||
                Level.Paused ||
                waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        /// <summary>
        /// Handles the enemy being killed by the player.
        /// </summary>
        /// <param name="killedBy">The player who killed the enemy.</param>
        public void OnKilled(Player killedBy)
        {
            isAlive = false;
            killedSound.Play();
        }
    }
}