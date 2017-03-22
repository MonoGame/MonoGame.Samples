using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spacewar
{
    public class KeyboardHelper
    {
        KeyboardState previousKeyState;
        public KeyboardState currentKeyState {get;private set;}

        public void Update(Game game, KeyboardState keyState)
        {
            previousKeyState = currentKeyState;
            currentKeyState = keyState;
        }

        public bool IsKeyPressed(Keys key)
        {
            if (previousKeyState.IsKeyUp(key) && currentKeyState.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }

        public bool IsKeyHeld(Keys key)
        {
            if (previousKeyState.IsKeyDown(key) && currentKeyState.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }

        public bool IsKeyReleased(Keys key)
        {
            if (previousKeyState.IsKeyDown(key) && currentKeyState.IsKeyUp(key))
            {
                return true;
            }
            return false;
        }
    }
}
