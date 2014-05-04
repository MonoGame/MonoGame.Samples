using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer2D
{
    class VirtualGamePad
    {
        private readonly Vector2 baseScreenSize;
        private Matrix globalTransformation;
        private readonly Texture2D texture;

        //Create these once only as we don't change them
        readonly GamePadThumbSticks thumbSticks = new GamePadThumbSticks();
        readonly GamePadTriggers triggers = new GamePadTriggers();
        readonly GamePadDPad dPad = new GamePadDPad();

        public VirtualGamePad(Vector2 baseScreenSize, Matrix globalTransformation, Texture2D texture)
        {
            this.baseScreenSize = baseScreenSize;
            this.globalTransformation = Matrix.Invert(globalTransformation);
            this.texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var spriteCenter = new Vector2(64, 64);
            spriteBatch.Draw(texture, new Vector2(64, baseScreenSize.Y - 64), null, Color.White, -MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(192, baseScreenSize.Y - 64), null, Color.White, MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, new Vector2(baseScreenSize.X - 128, baseScreenSize.Y - 128), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        public GamePadState GetState(TouchCollection touchState)
        {
            Buttons buttonsPressed = 0;
            
            foreach (var touch in touchState)
            {
                if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
                {
                    //Scale the touch position to be in _baseScreenSize coordinates
                    Vector2 pos = touch.Position;
                    Vector2.Transform(ref pos, ref globalTransformation, out pos);

                    if (pos.X < 128)
                        buttonsPressed |= Buttons.DPadLeft;
                    else if (pos.X < 256)
                        buttonsPressed |= Buttons.DPadRight;
                    else if (pos.X > baseScreenSize.X - 128)
                        buttonsPressed |= Buttons.A;
                }
            }

            var buttons = new GamePadButtons(buttonsPressed);

            return new GamePadState(thumbSticks, triggers, buttons, dPad);
        }
    }
}
