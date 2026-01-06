using System;
using Platformer2D.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Core
{
    /// <summary>
    /// Controls playback of an Animation.
    /// </summary>
    struct AnimationPlayer
    {
        /// <summary>
        /// Gets the animation which is currently playing.
        /// </summary>
        public Animation Animation
        {
            get { return animation; }
        }
        Animation animation;

        /// <summary>
        /// Gets the index of the current frame in the animation.
        /// </summary>
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        /// <summary>
        /// The amount of time in seconds that the current frame has been shown for.
        /// </summary>
        private float time;

        /// <summary>
        /// Gets a texture origin at the bottom center of each frame.
        /// </summary>
        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        /// <summary>
        /// Begins or continues playback of an animation.
        /// </summary>
        public void PlayAnimation(Animation animation)
        {
            // If this animation is already running, do not restart it.
            if (Animation == animation)
                return;

            // Start the new animation.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        /// <summary>
        /// Draws the current frame of the animation at the specified position using the default color (white).
        /// </summary>
        /// <param name="gameTime">Provides the elapsed game time for animation timing.</param>
        /// <param name="spriteBatch">The SpriteBatch used to draw the animation.</param>
        /// <param name="position">The screen position where the animation should be drawn.</param>
        /// <param name="spriteEffects">Specifies effects to apply (e.g., flip horizontally).</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            Draw(gameTime, spriteBatch, position, spriteEffects, Color.White);
        }

        /// <summary>
        /// Advances the animation's time position and draws the current frame at the specified position.
        /// </summary>
        /// <param name="gameTime">Provides the elapsed game time for animation timing.</param>
        /// <param name="spriteBatch">The SpriteBatch used to draw the animation.</param>
        /// <param name="position">The screen position where the animation should be drawn.</param>
        /// <param name="spriteEffects">Specifies effects to apply (e.g., flip horizontally).</param>
        /// <param name="color">The color to tint the animation. Use <see cref="Color.White"/> for no tint.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if no animation is set in the <c>Animation</c> property.
        /// </exception>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
        {
            if (Animation == null)
                throw new NotSupportedException(Resources.ErrorNoAnimation);

            // Process the elapsed time to advance the animation
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Advance the frame if enough time has passed
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                // Loop the animation or clamp to the final frame
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

            // Determine the portion of the texture representing the current frame
            Rectangle source = new Rectangle(
                frameIndex * Animation.Texture.Height, // Horizontal offset per frame
                0,                                     // Top edge of the frame
                Animation.Texture.Height,              // Frame width (assuming square frames)
                Animation.Texture.Height               // Frame height
            );

            // Draw the current frame at the specified position
            spriteBatch.Draw(Animation.Texture, position, source, color, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
        }
    }
}