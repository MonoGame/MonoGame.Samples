using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Linq;



namespace Spacewar
{
    public class TouchUI
    {
        public const float UIElementSize = 90.0f;
        TouchCollection Touches;


        private enum Sticks
        {
            Player1 = 0,
            Player2 = 1,
            Count = 2,
        }

        // UI elements.
        private Viewport _Viewport;
        private Vector2[] _StickLocation = new Vector2[(int)Sticks.Count];
        private Texture2D[] _StickTexture = new Texture2D[(int)Sticks.Count];

        // Current touch action information.
        private int[] _CurrentTouchId = new int[(int)Sticks.Count];
        private Vector2[] _StartTouchPoint = new Vector2[(int)Sticks.Count];
        private bool[] _CurrentStickAcceptingInput = new bool[(int)Sticks.Count];

        // Current per-update values.
        private Vector2[] _CurrentMoveAmount = new Vector2[(int)Sticks.Count];

        public void Initialise(Game game)
        {
            Touches = TouchPanel.GetState();

            IGraphicsDeviceService device = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
            _Viewport = device.GraphicsDevice.Viewport;

            // Get the touch control locations.
            double offsetx = _Viewport.Width / 6.0;
            double offsety = _Viewport.Height / 6.0;

            _StickLocation[(int)Sticks.Player1] = new Vector2(
                (float)(offsetx),
                (float)(_Viewport.Height - offsety));

            _StickLocation[(int)Sticks.Player2] = new Vector2(
                (float)(_Viewport.Width - offsetx),
                (float)(offsety));

            // Setup the UI.
            _StickTexture[(int)Sticks.Player1] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + "UI/RightThumbstick");
            _StickTexture[(int)Sticks.Player2] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + "UI/RightThumbstick");
        }

        private void ProcessTouchEvents(Sticks stick)
        {
            TouchLocation location = new TouchLocation();

            // Stick is not yet accepting input?
            if (!_CurrentStickAcceptingInput[(int)stick])
            {
                // No, so try to find an initial touch that is close enough to the stick.
                if (!FindInitialTouchEvent(_StickLocation[(int)stick], ref location))
                    return;

                _StartTouchPoint[(int)stick] = new Vector2(location.Position.X, location.Position.Y);
                _CurrentTouchId[(int)stick] = location.Id;
                _CurrentStickAcceptingInput[(int)stick] = true;
            }
            else
            {
                // Yes, so try to find any updated touch events for this id.
                if (!FindNextTouchEvent(_CurrentTouchId[(int)stick], ref location))
                    return;

                if (location.State == TouchLocationState.Released)
                {
                    _CurrentStickAcceptingInput[(int)stick] = false;
                    _CurrentMoveAmount[(int)stick] = Vector2.Zero;
                }
                else
                {
                    _CurrentMoveAmount[(int)stick].X = (float)(location.Position.X - _StartTouchPoint[(int)stick].X) * 0.01f;
                    _CurrentMoveAmount[(int)stick].Y = (float)(location.Position.Y - _StartTouchPoint[(int)stick].Y) * -0.01f;
                }
            }
        }

        public Vector2 GetStickMovement(PlayerIndex player)
        {
            return _CurrentMoveAmount[(int)player];
        }

        private bool FindInitialTouchEvent(Vector2 hotspotcenter, ref TouchLocation location)
        {
            foreach (TouchLocation testlocation in Touches)
            {
                // Only accept from new touches.
                if (testlocation.State != TouchLocationState.Pressed)
                    continue;

                testlocation.Position.Normalize();
                // Check distance to the hotspot.

                if (Vector2.Distance(testlocation.Position, hotspotcenter) >= 60.0f)
                    continue;

                location = testlocation;
                return true;
            }

            return false;
        }

        private bool FindNextTouchEvent(int touchid, ref TouchLocation location)
        {
            foreach (TouchLocation testlocation in Touches)
            {
                // Check distance to the hotspot.
                if (testlocation.Id != touchid)
                    continue;

                location = testlocation;
                return true;
            }

            return false;
        }

        public Vector2 StickPosition(PlayerIndex player)
        {
            switch (player)
            {
                case PlayerIndex.One:
                case PlayerIndex.Two:
                    return _CurrentMoveAmount[(int)player];
                default:
                    break;
            }
            return Vector2.Zero;
        }

        public void Update()
        {
            Touches = TouchPanel.GetState();


            // Convert events to movement amounts.
            ProcessTouchEvents(Sticks.Player1);
            ProcessTouchEvents(Sticks.Player2);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 sticksize = new Vector2(UIElementSize, UIElementSize);

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);

            //spriteBatch.Draw(_StickTexture[(int)Sticks.Left], _StickLocation[(int)Sticks.Left], sticksize, Color.White, true);
            //spriteBatch.Draw(_StickTexture[(int)Sticks.Right], _StickLocation[(int)Sticks.Right], sticksize, Color.White, true);
            spriteBatch.Draw(_StickTexture[(int)Sticks.Player1], _StickLocation[(int)Sticks.Player1].GetDrawCenter(_StickTexture[(int)Sticks.Player1]), Color.White);
            spriteBatch.Draw(_StickTexture[(int)Sticks.Player2], _StickLocation[(int)Sticks.Player2].GetDrawCenter(_StickTexture[(int)Sticks.Player2]), Color.White);

            spriteBatch.End();
        }
    }

    public static class Vector2DrawExtension
    {
        public static Vector2 GetDrawCenter(this Vector2 input, Texture2D image)
        {
            var centreX = input.X - (image.Width / 2);
            var centreY = input.Y - (image.Height / 2);
            return new Vector2(centreX, centreY);
        }
    }
}
