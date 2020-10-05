//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace NeonShooter
{
    static class Input
	{
		private static KeyboardState keyboardState, lastKeyboardState;
		private static MouseState mouseState, lastMouseState;
		private static GamePadState gamepadState, lastGamepadState;

		private static bool isAimingWithMouse = false;

		public static Vector2 MousePosition { get { return new Vector2(mouseState.X, mouseState.Y); } }

		public static void Update()
		{
			lastKeyboardState = keyboardState;
			lastMouseState = mouseState;
			lastGamepadState = gamepadState;

			keyboardState = Keyboard.GetState();
			mouseState = Mouse.GetState();
			gamepadState = GamePad.GetState(PlayerIndex.One);

			// If the player pressed one of the arrow keys or is using a gamepad to aim, we want to disable mouse aiming. Otherwise,
			// if the player moves the mouse, enable mouse aiming.
			if (new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down }.Any(x => keyboardState.IsKeyDown(x)) || gamepadState.ThumbSticks.Right != Vector2.Zero)
				isAimingWithMouse = false;
			else if (MousePosition != new Vector2(lastMouseState.X, lastMouseState.Y))
				isAimingWithMouse = true;
		}

		// Checks if a key was just pressed down
		public static bool WasKeyPressed(Keys key)
		{
			return lastKeyboardState.IsKeyUp(key) && keyboardState.IsKeyDown(key);
		}

		public static bool WasButtonPressed(Buttons button)
		{
			return lastGamepadState.IsButtonUp(button) && gamepadState.IsButtonDown(button);
		}

		public static Vector2 GetMovementDirection()
		{
			
			Vector2 direction = gamepadState.ThumbSticks.Left;
			direction.Y *= -1;	// invert the y-axis

			if (keyboardState.IsKeyDown(Keys.A))
				direction.X -= 1;
			if (keyboardState.IsKeyDown(Keys.D))
				direction.X += 1;
			if (keyboardState.IsKeyDown(Keys.W))
				direction.Y -= 1;
			if (keyboardState.IsKeyDown(Keys.S))
				direction.Y += 1;

			// Clamp the length of the vector to a maximum of 1.
			if (direction.LengthSquared() > 1)
				direction.Normalize();

			return direction;
		}

		public static Vector2 GetAimDirection()
		{
			if (isAimingWithMouse)
				return GetMouseAimDirection();

			Vector2 direction = gamepadState.ThumbSticks.Right;
			direction.Y *= -1;

			if (keyboardState.IsKeyDown(Keys.Left))
				direction.X -= 1;
			if (keyboardState.IsKeyDown(Keys.Right))
				direction.X += 1;
			if (keyboardState.IsKeyDown(Keys.Up))
				direction.Y -= 1;
			if (keyboardState.IsKeyDown(Keys.Down))
				direction.Y += 1;

			// If there's no aim input, return zero. Otherwise normalize the direction to have a length of 1.
			if (direction == Vector2.Zero)
				return Vector2.Zero;
			else
				return Vector2.Normalize(direction);
		}

		private static Vector2 GetMouseAimDirection()
		{
			Vector2 direction = MousePosition - PlayerShip.Instance.Position;

			if (direction == Vector2.Zero)
				return Vector2.Zero;
			else
				return Vector2.Normalize(direction);
		}

		public static bool WasBombButtonPressed()
		{
			return WasButtonPressed(Buttons.LeftTrigger) || WasButtonPressed(Buttons.RightTrigger) || WasKeyPressed(Keys.Space);
		}
	}
}