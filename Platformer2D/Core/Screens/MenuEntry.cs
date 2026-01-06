using System;
using Platformer2D.ScreenManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Screens;

/// <summary>
/// Helper class represents a single entry in a MenuScreen. By default this
/// just draws the entry text string, but it can be customized to display menu
/// entries in different ways. This also provides an event that will be raised
/// when the menu entry is selected.
/// </summary>
class MenuEntry
{
    /// <summary>
    /// Tracks a fading selection effect on the entry.
    /// </summary>
    /// <remarks>
    /// The entries transition out of the selection effect when they are deselected.
    /// </remarks>
    float selectionFade;

    string text;
    /// <summary>
    /// Gets or sets the text of this menu entry.
    /// </summary>
    public string Text
    {
        get { return text; }
        set { text = value; }
    }

    Vector2 position;
    /// <summary>
    /// The position at which the entry is drawn. This is set by the MenuScreen
    /// each frame in Update.
    /// </summary>
    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    private bool enabled;
    /// <summary>
    /// Whether this menu option is enabled or not.
    /// If not it will be displayed but skipped over during navigation
    /// </summary>
    public bool Enabled
    {
        get { return enabled; }
        set { enabled = value; }
    }

    /// <summary>
    /// Event raised when the menu entry is selected.
    /// </summary>
    public event EventHandler<PlayerIndexEventArgs> Selected;

    /// <summary>
    /// Method for raising the Selected event.
    /// </summary>
    /// <param name="playerIndex">The player index that selected the entry.</param>
    protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
    {
        Selected?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
    }

    /// <summary>
    /// Constructs a new menu entry with the specified text.
    /// </summary>
    /// <param name="text">The text to display for the menu entry.</param>
    /// <param name="enabled">Indicates whether the menu entry is enabled.</param>
    public MenuEntry(string text, bool enabled = true)
    {
        this.text = text;
        this.enabled = enabled;
    }

    /// <summary>
    /// Updates the menu entry's visual state.
    /// </summary>
    /// <param name="screen">The menu screen containing this entry.</param>
    /// <param name="isSelected">Indicates whether the entry is currently selected.</param>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        // When the menu selection changes, entries gradually fade between
        // their selected and deselected appearance, rather than instantly
        // popping to the new state.
        float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

        if (isSelected)
            selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
        else
            selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
    }


    /// <summary>
    /// Draws the menu entry. This can be overridden to customize the appearance.
    /// </summary>
    /// <param name="screen">The menu screen containing this entry.</param>
    /// <param name="isSelected">Indicates whether the entry is currently selected.</param>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        Color color;
        if (enabled)
        {
            // Draw the selected entry in yellow, otherwise white.
            color = isSelected ? Color.Yellow : Color.White;
        }
        else
        {
            color = Color.Gray;
        }

        // Pulsate the size of the selected menu entry.
        double time = gameTime.TotalGameTime.TotalSeconds;

        float pulsate = (float)Math.Sin(time * 6) + 1;

        float scale = 1 + pulsate * 0.05f * selectionFade;

        // Modify the alpha to fade text out during transitions.
        color *= screen.TransitionAlpha;

        // Draw text, centered on the middle of each line.
        ScreenManager screenManager = screen.ScreenManager;
        SpriteBatch spriteBatch = screenManager.SpriteBatch;
        SpriteFont font = screenManager.Font;

        Vector2 origin = new Vector2(0, font.LineSpacing / 2);

        spriteBatch.DrawString(font, text, position, color, 0,
                               origin, scale, SpriteEffects.None, 0);
    }

    /// <summary>
    /// Queries how much vertical space this menu entry requires.
    /// </summary>
    /// <param name="screen">The menu screen containing this entry.</param>
    /// <returns>The height of the menu entry in pixels.</returns>
    public virtual int GetHeight(MenuScreen screen)
    {
        return screen.ScreenManager.Font.LineSpacing;
    }

    /// <summary>
    /// Queries how much horizontal space this menu entry requires.
    /// </summary>
    /// <param name="screen">The menu screen containing this entry.</param>
    /// <returns>The width of the menu entry in pixels.</returns>
    public virtual int GetWidth(MenuScreen screen)
    {
        return (int)screen.ScreenManager.Font.MeasureString(Text).X;
    }
}