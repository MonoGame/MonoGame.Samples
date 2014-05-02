using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer2D
{
    class VirtualGamePad
    {
        private readonly Texture2D _texture;

        //Create these once only as we don't change them
        readonly GamePadThumbSticks thumbSticks = new GamePadThumbSticks();
        readonly GamePadTriggers triggers = new GamePadTriggers();
        readonly GamePadDPad dPad = new GamePadDPad();

        public VirtualGamePad(Texture2D texture)
        {
            _texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var spriteCenter = new Vector2(64, 64);
            spriteBatch.Draw(_texture, new Vector2(64, 480 - 64), null, Color.White, -MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(_texture, new Vector2(192, 480 - 64), null, Color.White, MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(_texture, new Vector2(800 - 128, 480 - 128), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public GamePadState GetState(TouchCollection touchState)
        {
            Buttons buttonsPressed = 0;
            
            foreach (var touch in touchState)
            {
                if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
                {
                    //TODO, touch locations need to be scaled
                    if (touch.Position.X < 128)
                        buttonsPressed |= Buttons.DPadLeft;
                    else if (touch.Position.X < 256)
                        buttonsPressed |= Buttons.DPadRight;
                    else if (touch.Position.X > 800 - 128)
                        buttonsPressed |= Buttons.A;
                }
            }

            var buttons = new GamePadButtons(buttonsPressed);

            return new GamePadState(thumbSticks, triggers, buttons, dPad);
        }
    }
}
