using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace myStartkit.Core.Inputs;

/// <summary>
/// Helper for reading input from keyboard, gamepad, and touch input. This class 
/// tracks both the current and previous state of the input devices, and implements 
/// query methods for high level input actions such as "move up through the menu"
/// or "pause the game".
/// </summary>
public class InputState
{
    public const int MaxInputs = 4; // Maximum number of supported input devices (e.g., players)

    // Current Inputstates - Tracks the latest state of all input devices
    public readonly GamePadState[] CurrentGamePadStates;
    public readonly KeyboardState[] CurrentKeyboardStates;
    public MouseState CurrentMouseState;
    private int touchCount; // Number of active touch inputs
    public TouchCollection CurrentTouchState;

    // Last Inputstates - Stores the previous frame's input states for detecting changes
    public readonly GamePadState[] LastGamePadStates;
    public readonly KeyboardState[] LastKeyboardStates;
    public MouseState LastMouseState;
    public TouchCollection LastTouchState;

    public readonly List<GestureSample> Gestures = new List<GestureSample>(); // Stores touch gestures

    /// <summary>
    /// Cursor move speed in pixels per second
    /// </summary>
    private const float cursorMoveSpeed = 250.0f;

    private Vector2 currentCursorLocation;
    /// <summary>
    /// Current location of our Cursor
    /// </summary>
    public Vector2 CurrentCursorLocation => currentCursorLocation;

    private Vector2 lastCursorLocation;
    /// <summary>
    /// Current location of our Cursor
    /// </summary>
    public Vector2 LastCursorLocation => lastCursorLocation;

    private bool isMouseWheelScrolledDown;
    /// <summary>
    /// Has the user scrolled the mouse wheel down?
    /// </summary>
    public bool IsMouseWheelScrolledDown => isMouseWheelScrolledDown;

    private bool isMouseWheelScrolledUp;
    private Matrix inputTransformation; // Used to transform input coordinates between screen and game space

    /// <summary>
    /// Has the user scrolled the mouse wheel up?
    /// </summary>
    public bool IsMouseWheelScrolledUp => isMouseWheelScrolledUp;

    /// <summary>
    /// Constructs a new input state.
    /// </summary>
    public InputState()
    {
        // Initialize arrays for multiple controller/keyboard states
        CurrentKeyboardStates = new KeyboardState[MaxInputs];
        CurrentGamePadStates = new GamePadState[MaxInputs];

        LastKeyboardStates = new KeyboardState[MaxInputs];
        LastGamePadStates = new GamePadState[MaxInputs];
    }

    /// <summary>
    /// Reads the latest state of all the inputs.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="viewport">The viewport to constrain cursor movement within.</param>
    public void Update(GameTime gameTime, Viewport viewport)
    {
        // Update keyboard and gamepad states for all players
        for (int i = 0; i < MaxInputs; i++)
        {
            LastKeyboardStates[i] = CurrentKeyboardStates[i];
            LastGamePadStates[i] = CurrentGamePadStates[i];

            CurrentKeyboardStates[i] = Keyboard.GetState();
            CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
        }

        // Update mouse state
        LastMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();

        // Update touch state
        touchCount = 0;
        LastTouchState = CurrentTouchState;
        CurrentTouchState = TouchPanel.GetState();

        // Process all available gestures
        Gestures.Clear();
        while (TouchPanel.IsGestureAvailable)
        {
            Gestures.Add(TouchPanel.ReadGesture());
        }

        // Process touch inputs
        foreach (TouchLocation location in CurrentTouchState)
        {
            switch (location.State)
            {
                case TouchLocationState.Pressed:
                    touchCount++;
                    lastCursorLocation = currentCursorLocation;
                    // Transform touch position to game coordinates
                    currentCursorLocation = TransformCursorLocation(location.Position);
                    break;
                case TouchLocationState.Moved:
                    break;
                case TouchLocationState.Released:
                    break;
            }
        }

        // Handle mouse clicks as touch equivalents
        if (IsLeftMouseButtonClicked())
        {
            lastCursorLocation = currentCursorLocation;
            // Transform mouse position to game coordinates
            currentCursorLocation = TransformCursorLocation(new Vector2(CurrentMouseState.X, CurrentMouseState.Y));
            touchCount = 1;
        }

        if (IsMiddleMouseButtonClicked())
        {
            touchCount = 2; // Treat middle mouse click as double touch
        }

        if (IsRightMoustButtonClicked())
        {
            touchCount = 3; // Treat right mouse click as triple touch
        }

        // Reset mouse wheel flags
        isMouseWheelScrolledUp = false;
        isMouseWheelScrolledDown = false;

        // Detect mouse wheel scrolling
        if (CurrentMouseState.ScrollWheelValue != LastMouseState.ScrollWheelValue)
        {
            int scrollWheelDelta = CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue;

            // Handle the scroll wheel event based on the delta
            if (scrollWheelDelta > 0)
            {
                // Mouse wheel scrolled up
                isMouseWheelScrolledUp = true;
            }
            else if (scrollWheelDelta < 0)
            {
                // Mouse wheel scrolled down
                isMouseWheelScrolledDown = true;
            }
        }

        // Update the cursor location using gamepad and keyboard
        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Move cursor with gamepad thumbstick
        if (CurrentGamePadStates[0].IsConnected)
        {
            lastCursorLocation = currentCursorLocation;

            currentCursorLocation.X += CurrentGamePadStates[0].ThumbSticks.Left.X * elapsedTime * cursorMoveSpeed;
            currentCursorLocation.Y -= CurrentGamePadStates[0].ThumbSticks.Left.Y * elapsedTime * cursorMoveSpeed;
        }

        // Move cursor with keyboard arrow keys
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Up))
        {
            currentCursorLocation.Y -= elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Down))
        {
            currentCursorLocation.Y += elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Left))
        {
            currentCursorLocation.X -= elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Right))
        {
            currentCursorLocation.X += elapsedTime * cursorMoveSpeed;
        }

        // Keep cursor within viewport bounds
        currentCursorLocation.X = MathHelper.Clamp(currentCursorLocation.X, 0f, viewport.Width);
        currentCursorLocation.Y = MathHelper.Clamp(currentCursorLocation.Y, 0f, viewport.Height);
    }

    /// <summary>
    /// Checks if left mouse button was clicked (pressed and then released)
    /// </summary>
    /// <returns>True if left mouse button was clicked, false otherwise.</returns>
    internal bool IsLeftMouseButtonClicked()
    {
        return CurrentMouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if middle mouse button was clicked (pressed and then released)
    /// </summary>
    /// <returns>True if middle mouse button was clicked, false otherwise.</returns>
    internal bool IsMiddleMouseButtonClicked()
    {
        return CurrentMouseState.MiddleButton == ButtonState.Released && LastMouseState.MiddleButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if right mouse button was clicked (pressed and then released)
    /// </summary>
    /// <returns>True if right mouse button was clicked, false otherwise.</returns>
    internal bool IsRightMoustButtonClicked()
    {
        return CurrentMouseState.RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Helper for checking if a key was newly pressed during this update.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player pressed the key.</param>
    /// <returns>True if the key was newly pressed, false otherwise.</returns>
    public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                        out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            // Read input from the specified player.
            playerIndex = controllingPlayer.Value;

            int i = (int)playerIndex;

            return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                    LastKeyboardStates[i].IsKeyUp(key));
        }
        else
        {
            // Accept input from any player.
            return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
        }
    }


    /// <summary>
    /// Helper for checking if a button was newly pressed during this update.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player pressed the button.</param>
    /// <returns>True if the button was newly pressed, false otherwise.</returns>
    public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                 out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            // Read input from the specified player.
            playerIndex = controllingPlayer.Value;

            int i = (int)playerIndex;

            return (CurrentGamePadStates[i].IsButtonDown(button) &&
                    LastGamePadStates[i].IsButtonUp(button));
        }
        else
        {
            // Accept input from any player.
            return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
        }
    }


    /// <summary>
    /// Checks for a "menu select" input action.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player triggered the action.</param>
    /// <returns>True if menu select action occurred, false otherwise.</returns>
    public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                             out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
               IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
    }


    /// <summary>
    /// Checks for a "menu cancel" input action.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="playerIndex">Outputs which player triggered the action.</param>
    /// <returns>True if menu cancel action occurred, false otherwise.</returns>
    public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                             out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
    }


    /// <summary>
    /// Checks for a "menu up" input action.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns>True if menu up action occurred, false otherwise.</returns>
    public bool IsMenuUp(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex) ||
               IsMouseWheelScrolledUp;
    }


    /// <summary>
    /// Checks for a "menu down" input action.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns>True if menu down action occurred, false otherwise.</returns>
    public bool IsMenuDown(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex) ||
               IsMouseWheelScrolledDown;
    }


    /// <summary>
    /// Checks for a "pause the game" input action.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <param name="rectangle">Optional rectangle to check for clicks within.</param>
    /// <returns>True if pause action occurred, false otherwise.</returns>
    public bool IsPauseGame(PlayerIndex? controllingPlayer, Rectangle? rectangle = null)
    {
        PlayerIndex playerIndex;

        bool pointInRect = false;

        // Check if the cursor is in the provided rectangle and was clicked
        if (rectangle.HasValue)
        {
            if (rectangle.Value.Contains(CurrentCursorLocation)
                && (IsLeftMouseButtonClicked() || touchCount > 0))
            {
                pointInRect = true;
            }
        }

        return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex)
            || IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex)
            || IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex)
            || pointInRect;
    }

    /// <summary>
    /// Checks if player has selected next on either keyboard or gamepad.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns>True if select next action occurred, false otherwise.</returns>
    public bool IsSelectNext(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex);
    }

    /// <summary>
    /// Checks if player has selected previous on either keyboard or gamepad.
    /// </summary>
    /// <param name="controllingPlayer">The player to read input for, or null for any player.</param>
    /// <returns>True if select previous action occurred, false otherwise.</returns>
    public bool IsSelectPrevious(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex);
    }

    /// <summary>
    /// Updates the matrix used to transform input coordinates.
    /// </summary>
    /// <param name="inputTransformation">The transformation matrix to apply.</param>
    internal void UpdateInputTransformation(Matrix inputTransformation)
    {
        this.inputTransformation = inputTransformation;
    }

    /// <summary>
    /// Transforms touch/mouse positions from screen space to game space.
    /// </summary>
    /// <param name="mousePosition">The screen-space position to transform.</param>
    /// <returns>The transformed position in game space.</returns>
    public Vector2 TransformCursorLocation(Vector2 mousePosition)
    {
        // Transform back to cursor location
        return Vector2.Transform(mousePosition, inputTransformation);
    }

    /// <summary>
    /// Checks if a UI element was clicked, either by mouse or touch.
    /// </summary>
    /// <param name="rectangle">The rectangle bounds of the UI element to check.</param>
    /// <returns>True if the UI element was clicked, false otherwise.</returns>
    internal bool IsUIClicked(Rectangle rectangle)
    {
        bool pointInRect = false;

        if (rectangle.Contains(CurrentCursorLocation)
            && (IsLeftMouseButtonClicked() || touchCount > 0))
        {
            pointInRect = true;
        }

        return pointInRect;
    }
}