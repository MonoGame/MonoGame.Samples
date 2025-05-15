using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Input;

public class InputManager
{
    /// <summary>
    /// Gets the state information of keyboard input.
    /// </summary>
    public KeyboardInfo Keyboard { get; private set; }

    /// <summary>
    /// Gets the state information of mouse input.
    /// </summary>
    public MouseInfo Mouse { get; private set; }

    /// <summary>
    /// Gets the state information of a gamepad.
    /// </summary>
    public GamePadInfo[] GamePads { get; private set; }

    /// <summary>
    /// Creates a new InputManager.
    /// </summary>
    /// <param name="game">The game this input manager belongs to.</param>
    public InputManager()
    {
        Keyboard = new KeyboardInfo();
        Mouse = new MouseInfo();

        GamePads = new GamePadInfo[4];
        for (int i = 0; i < 4; i++)
        {
            GamePads[i] = new GamePadInfo((PlayerIndex)i);
        }
    }

    /// <summary>
    /// Updates the state information for the keyboard, mouse, and gamepad inputs.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public void Update(GameTime gameTime)
    {
        Keyboard.Update();
        Mouse.Update();

        for (int i = 0; i < 4; i++)
        {
            GamePads[i].Update(gameTime);
        }
    }
}
