using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer2D.Core.Inputs;

/// <summary>
/// Provides an on-screen virtual gamepad interface for mobile or touch-based platforms.
/// This class handles fading in/out visual controls, tracking touch input, and generating
/// corresponding <see cref="GamePadState"/> for seamless integration with existing game logic.
/// </summary>
class VirtualGamePad
{
    private readonly Vector2 baseScreenSize;
    private Matrix globalTransformation;
    private readonly Texture2D texture;

    private float secondsSinceLastInput;
    private float opacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualGamePad"/> class.
    /// </summary>
    /// <param name="baseScreenSize">The base screen size used to scale touch input.</param>
    /// <param name="globalTransformation">
    /// A transformation matrix used to scale touch coordinates to match the base screen size.
    /// </param>
    /// <param name="texture">The texture representing the visual gamepad controls.</param>
    public VirtualGamePad(Vector2 baseScreenSize, Matrix globalTransformation, Texture2D texture)
    {
        this.baseScreenSize = baseScreenSize;
        this.globalTransformation = Matrix.Invert(globalTransformation); // Inverted for touch-to-screen conversion
        this.texture = texture;
        secondsSinceLastInput = float.MaxValue; // Ensures controls are initially faded out
    }

    /// <summary>
    /// Notifies the virtual gamepad that the player is actively moving.
    /// Resets the input timer to trigger a fade-out effect for the controls.
    /// </summary>
    public void NotifyPlayerIsMoving()
    {
        secondsSinceLastInput = 0;
    }

    /// <summary>
    /// Updates the opacity of the visual gamepad controls based on recent player activity.
    /// </summary>
    /// <param name="gameTime">Provides timing values for the game loop.</param>
    public void Update(GameTime gameTime)
    {
        var secondsElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        secondsSinceLastInput += secondsElapsed;

        // Fade-out logic: If the player is moving, reduce opacity quickly
        // Otherwise, after 4 seconds of inactivity, fade the controls back in
        if (secondsSinceLastInput < 4)
            opacity = Math.Max(0, opacity - secondsElapsed * 4);
        else
            opacity = Math.Min(1, opacity + secondsElapsed * 2);
    }

    /// <summary>
    /// Draws the visual gamepad controls on the screen with the appropriate opacity.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to draw the textures.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        var spriteCenter = new Vector2(64, 64); // Texture's visual center for rotation pivot
        var color = Color.Multiply(Color.White, opacity);

        // Draw directional controls
        spriteBatch.Draw(texture, new Vector2(64, baseScreenSize.Y - 64), null, color, -MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
        spriteBatch.Draw(texture, new Vector2(192, baseScreenSize.Y - 64), null, color, MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);

        // Draw the primary action button (e.g., jump/attack)
        spriteBatch.Draw(texture, new Vector2(baseScreenSize.X - 128, baseScreenSize.Y - 128), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
    }

    /// <summary>
    /// Generates a <see cref="GamePadState"/> based on touch input and the current physical gamepad state.
    /// </summary>
    /// <param name="touchState">The touch collection representing active touch points.</param>
    /// <param name="gpState">The current state of the physical gamepad.</param>
    /// <returns>A combined <see cref="GamePadState"/> reflecting both touch and physical input.</returns>
    public GamePadState GetState(TouchCollection touchState, GamePadState gpState)
    {
        Buttons buttonsPressed = 0;

        // Evaluate touch input to determine virtual button presses
        foreach (var touch in touchState)
        {
            if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
            {
                Vector2 pos = touch.Position;
                Vector2.Transform(ref pos, ref globalTransformation, out pos);

                if (pos.X < 128)
                    buttonsPressed |= Buttons.DPadLeft;
                else if (pos.X < 256)
                    buttonsPressed |= Buttons.DPadRight;
                else if (pos.X >= baseScreenSize.X - 128)
                    buttonsPressed |= Buttons.A;
            }
        }

        // Combine real gamepad inputs with virtual gamepad inputs
        var gpButtons = gpState.Buttons;
        buttonsPressed |= gpButtons.A == ButtonState.Pressed ? Buttons.A : 0;
        buttonsPressed |= gpButtons.B == ButtonState.Pressed ? Buttons.B : 0;
        buttonsPressed |= gpButtons.X == ButtonState.Pressed ? Buttons.X : 0;
        buttonsPressed |= gpButtons.Y == ButtonState.Pressed ? Buttons.Y : 0;

        buttonsPressed |= gpButtons.Start == ButtonState.Pressed ? Buttons.Start : 0;
        buttonsPressed |= gpButtons.Back == ButtonState.Pressed ? Buttons.Back : 0;

        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadDown) ? Buttons.DPadDown : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadLeft) ? Buttons.DPadLeft : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadRight) ? Buttons.DPadRight : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadUp) ? Buttons.DPadUp : 0;

        buttonsPressed |= gpButtons.BigButton == ButtonState.Pressed ? Buttons.BigButton : 0;
        buttonsPressed |= gpButtons.LeftShoulder == ButtonState.Pressed ? Buttons.LeftShoulder : 0;
        buttonsPressed |= gpButtons.RightShoulder == ButtonState.Pressed ? Buttons.RightShoulder : 0;

        buttonsPressed |= gpButtons.LeftStick == ButtonState.Pressed ? Buttons.LeftStick : 0;
        buttonsPressed |= gpButtons.RightStick == ButtonState.Pressed ? Buttons.RightStick : 0;

        // Create a new GamePadState with the combined inputs
        var buttons = new GamePadButtons(buttonsPressed);
        return new GamePadState(gpState.ThumbSticks, gpState.Triggers, buttons, gpState.DPad);
    }
}