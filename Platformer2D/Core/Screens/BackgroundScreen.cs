using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Screens;

/// <summary>
/// The BackgroundScreen renders a static background image behind all other menu screens.
/// It remains fixed and unaffected by transitions on top of it.
/// </summary>
class BackgroundScreen : GameScreen
{
    ContentManager content;
    Texture2D backgroundTexture;

    /// <summary>
    /// Initializes a new instance of the BackgroundScreen class.
    /// Sets the transition times for screen appearance and disappearance.
    /// </summary>
    public BackgroundScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    /// <summary>
    /// Loads the background texture using a local ContentManager.
    /// This allows for independent unloading of the background texture.
    /// </summary>
    public override void LoadContent()
    {
        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        backgroundTexture = content.Load<Texture2D>("Backgrounds/Layer0_2");
    }

    /// <summary>
    /// Unloads the background texture by unloading the local ContentManager.
    /// </summary>
    public override void UnloadContent()
    {
        content.Unload();
    }

    /// <summary>
    /// Updates the background screen.
    /// Forces the screen to remain active even when covered by other screens.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="otherScreenHasFocus">Indicates whether another screen has focus.</param>
    /// <param name="coveredByOtherScreen">Indicates whether the screen is covered by another screen.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        // Force coveredByOtherScreen to false to prevent the screen from transitioning off.
        base.Update(gameTime, otherScreenHasFocus, false);
    }

    /// <summary>
    /// Draws the background screen.
    /// Clears the screen and renders the background texture with transition alpha.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public override void Draw(GameTime gameTime)
    {
        // Clear the screen to black to prevent visual artifacts.
        ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Rectangle fullscreen = new Rectangle(0, 0, (int)ScreenManager.BaseScreenSize.X, (int)ScreenManager.BaseScreenSize.Y);

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        // Draw the background texture with the current transition alpha.
        spriteBatch.Draw(backgroundTexture, fullscreen, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

        spriteBatch.End();
    }
}