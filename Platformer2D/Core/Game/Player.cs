using System;
using Platformer2D.Core.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer2D.Core
{
    /// <summary>
    /// Our fearless adventurer!
    /// Handles movement, physics, collisions, animations, and player states.
    /// </summary>
    class Player
    {
        // ==================== Animation Properties ====================
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;


        // Determines if the sprite is flipped horizontally based on movement direction
        private SpriteEffects flip = SpriteEffects.None;

        // Manages the current animation being played
        private AnimationPlayer sprite;

        // ==================== Sound Effects ====================
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        private SoundEffect powerUpSound;

        private Level level;
        /// <summary>
        /// Gets the level that contains this player.
        /// </summary>
        public Level Level
        {
            get { return level; }
        }

        private bool isAlive;
        /// <summary>
        /// Gets whether the player is currently alive.
        /// </summary>
        public bool IsAlive
        {
            get { return isAlive; }
        }

        Vector2 position;
        /// <summary>
        /// Gets or sets the player's position in the world.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        // Stores the bottom position from the previous frame for platform collision detection
        private float previousBottom;

        /// <summary>
        /// Gets or sets the player's velocity vector.
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // ==================== Movement Constants ====================
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // ==================== Jump Constants ====================
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        // ==================== Input Configuration ====================
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        private bool isOnGround;
        /// <summary>
        /// Gets whether the player is currently standing on ground.
        /// Used to determine if player can jump and which animations to play.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }

        /// <summary>
        /// Horizontal movement input value. -1.0 for left, 1.0 for right, 0.0 for no movement.
        /// </summary>
        private float movement;
        public float Movement
        {
            get
            {
                return movement;
            }
            set
            {
                movement = value;
            }
        }

        // ==================== Jump State ====================
        /// <summary>
        /// Indicates if the player is attempting to jump in the current frame
        /// </summary>
        private bool isJumping;
        public bool IsJumping
        {
            get
            {
                return isJumping;
            }
            set
            {
                isJumping = value;
            }
        }

        private bool wasJumping;
        private float initialFallYPosition;
        private bool isFalling;
        private float jumpTime;
        private const float MaxSafeFallDistance = -250f;

        // ==================== Collision Detection ====================
        // Local bounds of the player sprite relative to the sprite origin
        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds the player in world space,
        /// used for collision detection with the level.
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

        // ==================== PowerUp State ====================
        private const float MaxPowerUpTime = 6.0f;

        private float powerUpTime;
        /// <summary>
        /// Gets whether the player currently has an active power-up
        /// </summary>
        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }

        // Current player mode (e.g., Playing, Scripted movement)
        public PlayerMode Mode { get; internal set; }

        // Colors used for the power-up visual effect, cycling through these creates a flashing effect
        // Could be stored in a file and read-in
        private readonly Color[] poweredUpColors = {
                               Color.Red,
                               Color.Blue,
                               Color.Orange,
                               Color.Yellow,
                                               };

        /// <summary>
        /// Constructs a new player character in the specified level at the given position.
        /// </summary>
        /// <param name="level">The level the player belongs to</param>
        /// <param name="position">The initial position in the level</param>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads all player-related content: sprites, animations, and sound effects.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.6f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);

            // Create collision bounds - smaller than the sprite for better gameplay feel
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerPowerUp");
        }

        /// <summary>
        /// Resets the player to life at the specified position.
        /// Called at the beginning of a level and when respawning after death.
        /// </summary>
        /// <param name="position">The position to respawn at</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Updates the player's state based on input, physics, and animations.
        /// Main update method called each frame.
        /// </summary>
        /// <param name="gameTime">Provides timing information</param>
        /// <param name="inputState">Current state of all input devices</param>
        /// <param name="displayOrientation">Orientation of the display for accelerometer adjustments</param>
        public void Update(
            GameTime gameTime,
            InputState inputState,
            DisplayOrientation displayOrientation)
        {
            // Only process input if the player is in playing mode
            if (Mode == PlayerMode.Playing)
                HandleInput(inputState, displayOrientation);

            Move(gameTime);
        }

        /// <summary>
        /// Updates player's position, applies physics, and updates animations.
        /// Called each frame after input is processed.
        /// </summary>
        /// <param name="gameTime">Provides timing information</param>
        public void Move(GameTime gameTime)
        {
            if (IsAlive)
            {
                ApplyPhysics(gameTime);

                // Update power-up timer
                if (IsPoweredUp)
                    powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

                // Play the appropriate animation based on player state
                if (IsOnGround)
                {
                    if (Math.Abs(Velocity.X) - 0.02f > 0)
                    {
                        sprite.PlayAnimation(runAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                }
            }

            // Reset input state for next frame
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Processes player input from keyboard, gamepad, accelerometer, and touch/mouse.
        /// Sets movement direction and jump state based on input.
        /// </summary>
        /// <param name="inputState">Current state of all input devices</param>
        /// <param name="displayOrientation">Orientation of the display for accelerometer adjustments</param>
        private void HandleInput(
            InputState inputState,
            DisplayOrientation displayOrientation)
        {
            // Get analog horizontal movement from gamepad
            movement = inputState.CurrentGamePadStates[0].ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent subtle drifting
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Process keyboard and D-pad input for movement
            if (inputState.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadLeft) ||
                inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.Left) ||
                inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (inputState.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadRight) ||
                     inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.Right) ||
                     inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check for jump input from gamepad or keyboard
            isJumping =
                inputState.CurrentGamePadStates[0].IsButtonDown(JumpButton) ||
                inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.Space) ||
                inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.Up) ||
                inputState.CurrentKeyboardStates[0].IsKeyDown(Keys.W);

            // Handle touch/mouse input if activated
            if (inputState.CurrentTouchState.Count > 0 || inputState.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                HandleClickInput(inputState.CurrentCursorLocation);
            }
        }

        /// <summary>
        /// Processes touch/mouse input for movement and jumping.
        /// Click/tap above player to jump, to the side to move.
        /// </summary>
        /// <param name="clickPosition">Screen coordinates of touch/click</param>
        private void HandleClickInput(Vector2 clickPosition)
        {
            // Get current player position for reference
            Vector2 playerPosition = Position;

            // Define thresholds for different input zones
            float jumpThresholdY = playerPosition.Y - 50; // Area above player for jumping
            float moveThresholdX = 20f; // Minimum distance for horizontal movement

            // Check if click is in the "jump zone" (above player)
            bool shouldJump = clickPosition.Y < jumpThresholdY;

            // Check if click is in "move right" or "move left" zones
            bool shouldMoveRight = clickPosition.X > playerPosition.X + moveThresholdX;
            bool shouldMoveLeft = clickPosition.X < playerPosition.X - moveThresholdX;

            // Apply the appropriate action based on zone
            if (shouldJump)
            {
                // Trigger jump
                isJumping = true;
            }
            else if (shouldMoveRight)
            {
                // Move right
                movement = 1.0f;
            }
            else if (shouldMoveLeft)
            {
                // Move left
                movement = -1.0f;
            }
            else
            {
                // No movement if clicked too close to player
                movement = 0.0f;
            }
        }

        /// <summary>
        /// Applies physics to update player's velocity and position.
        /// Handles gravity, jump physics, drag, and maximum velocity caps.
        /// </summary>
        /// <param name="gameTime">Provides timing information</param>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Store position before physics update for collision resolution
            Vector2 previousPosition = Position;

            // Apply horizontal acceleration based on input and vertical acceleration due to gravity
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            // Calculate jump physics if jumping
            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply drag to slow the player down (different values for ground and air)
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Cap horizontal speed to prevent excessive velocity
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Update position based on velocity
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // Check and resolve collisions with the level
            HandleCollisions();

            // If collision prevented movement, reset velocity component to zero
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity based on jump state and timing.
        /// Implements a variable-height jump with more control at the apex.
        /// Also handles fall detection and damage from excessive falls.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">Current vertical velocity</param>
        /// <param name="gameTime">Provides timing information</param>
        /// <returns>Updated vertical velocity</returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump - either just pressed jump on ground or holding jump in mid-jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    // Play jump sound when starting a new jump
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    // Track jump duration and play jump animation
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }

                // During the ascent phase of the jump (controlled by jump button duration)
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Apply a power curve that gives more control at the apex of the jump
                    // The longer the button is held, the higher the jump (up to a maximum)
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Jump button held too long or released, end the controlled jump phase
                    jumpTime = 0.0f;
                }

                // Reset fall tracking when jumping
                isFalling = false;
            }
            else
            {
                // Not jumping or canceled jump - reset jump timer
                jumpTime = 0.0f;

                // Detect when player starts falling (not on ground, not jumping, not already in fall state)
                if (!IsOnGround && !isJumping && !isFalling)
                {
                    // Record starting height of fall for damage calculation
                    initialFallYPosition = position.Y;
                    isFalling = true;
                }

                // If player lands after falling
                if (IsOnGround && isFalling)
                {
                    // Calculate total fall distance
                    float fallDistance = initialFallYPosition - position.Y;

                    // Apply fall damage if fall was too far
                    if (fallDistance < MaxSafeFallDistance)
                    {
                        OnKilled(null);
                    }

                    // Reset fall state after landing
                    isFalling = false;
                }
            }
            // Track jump button state for next frame
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves collisions between the player and level tiles.
        /// Handles different tile types (impassable, platform, breakable).
        /// </summary>
        private void HandleCollisions()
        {
            // Get player's collision rectangle and determine which tiles to check
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset ground detection for this frame
            isOnGround = false;

            // Check each potentially colliding tile
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // Skip non-collidable tiles
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Calculate overlap depth between player and tile
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);

                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve collision along the shallowest axis (usually gives better results)
                            // Platforms are special cases that only collide from above
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // Check if player is standing on this tile (previous bottom was above tile top)
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Only apply Y collision for impassable tiles or when on ground for platforms
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Push player out of collision along Y axis
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);
                                    bounds = BoundingRectangle; // Update bounds for subsequent checks
                                }

                                // Special handling for breakable tiles when hit from below
                                if (collision == TileCollision.Breakable && depth.Y < 0 && previousBottom > tileBounds.Top)
                                {
                                    level.BreakTile(x, y);
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Not for platforms
                            {
                                // Push player out of collision along X axis
                                Position = new Vector2(Position.X + depth.X, Position.Y);
                                bounds = BoundingRectangle; // Update bounds for subsequent checks
                            }
                        }
                    }
                }
            }

            // Kill player if they fall below the level
            if (BoundingRectangle.Top >= level.Height * Tile.Height)
                OnKilled(null);

            // Store bottom position for next frame's platform detection
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Handles player death, either from enemies or environmental hazards.
        /// Plays death animation and sound effect.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy that killed the player, or null if killed by falling or other hazard.
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            // Play appropriate death sound
            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            // Play death animation
            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when player reaches the level exit.
        /// Plays celebration animation.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the player with appropriate animation, facing direction, and color effects.
        /// </summary>
        /// <param name="gameTime">Provides timing information</param>
        /// <param name="spriteBatch">SpriteBatch used for drawing</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip sprite based on movement direction
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Apply color effects for power-up state
            Color color;
            if (IsPoweredUp)
            {
                // Cycle through power-up colors for flashing effect
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
                int colorIndex = (int)t % poweredUpColors.Length;
                color = poweredUpColors[colorIndex];
            }
            else
            {
                color = Color.White; // Normal color when not powered up
            }

            // Draw the player sprite with current animation, position, and effects
            sprite.Draw(gameTime, spriteBatch, Position, flip, color);
        }

        /// <summary>
        /// Activates power-up state for the player.
        /// Sets power-up timer and plays power-up sound effect.
        /// </summary>
        internal void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }
    }
}