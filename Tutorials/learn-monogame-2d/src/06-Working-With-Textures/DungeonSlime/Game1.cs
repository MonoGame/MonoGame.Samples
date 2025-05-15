using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;

namespace DungeonSlime;

public class Game1 : Core
{
    // The MonoGame logo texture
    private Texture2D _logo;

    public Game1() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _logo = Content.Load<Texture2D>("images/logo");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // The bounds of the icon within the texture.
        Rectangle iconSourceRect = new Rectangle(0, 0, 128, 128);

        // The bounds of the word mark within the texture.
        Rectangle wordmarkSourceRect = new Rectangle(150, 34, 458, 58);

        // Begin the sprite batch to prepare for rendering.
        SpriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack);

        // Draw only the icon portion of the texture.
        SpriteBatch.Draw(
            _logo,              // texture
            new Vector2(        // position
                Window.ClientBounds.Width,
                Window.ClientBounds.Height) * 0.5f,
            iconSourceRect,     // sourceRectangle
            Color.White,        // color
            0.0f,               // rotation
            new Vector2(        // origin
                iconSourceRect.Width,
                iconSourceRect.Height) * 0.5f,
            1.0f,               // scale
            SpriteEffects.None, // effects
            1.0f                // layerDepth
        );

        // Draw only the word mark portion of the texture.
        SpriteBatch.Draw(
            _logo,              // texture
            new Vector2(        // position
              Window.ClientBounds.Width,
              Window.ClientBounds.Height) * 0.5f,
            wordmarkSourceRect, // sourceRectangle
            Color.White,        // color
            0.0f,               // rotation
            new Vector2(        // origin
              wordmarkSourceRect.Width,
              wordmarkSourceRect.Height) * 0.5f,
            1.0f,               // scale
            SpriteEffects.None, // effects
            0.0f                // layerDepth
        );

        // Always end the sprite batch when finished.
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
