using System;
using System.Collections.Generic;
using Platformer2D.Core;
using Platformer2D.Core.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer2D.Screens;

/// <summary>
/// Base class for screens that contain a menu of options. The user can
/// move up and down to select an entry, or cancel to back out of the screen.
/// </summary>
abstract class MenuScreen : GameScreen
{
    private List<MenuEntry> menuEntries = new List<MenuEntry>();
    private int selectedEntry = 0;
    private string menuTitle;
    private Color menuTitleColor = new Color(0, 0, 0); // Default color is black. Use new Color(192, 192, 192) for off-white.

    /// <summary>
    /// Gets or sets the title of the menu screen.
    /// </summary>
    public string Title { get => menuTitle; set => menuTitle = value; }

    /// <summary>
    /// Gets the list of menu entries, so derived classes can add
    /// or change the menu contents.
    /// </summary>
    protected IList<MenuEntry> MenuEntries
    {
        get { return menuEntries; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuScreen"/> class.
    /// </summary>
    /// <param name="menuTitle">The title of the menu screen.</param>
    public MenuScreen(string menuTitle)
    {
        this.menuTitle = menuTitle;

        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    /// <summary>
    /// Loads content for the menu screen. This method is called once per game
    /// and is the place to load all content specific to the menu screen.
    /// </summary>
    public override void LoadContent()
    {
        base.LoadContent();
    }

    /// <summary>
    /// Responds to user input, changing the selected entry and accepting
    /// or canceling the menu.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="inputState">Provides the current state of input devices.</param>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        // Handle touch input for mobile platforms.
        if (Platformer2DGame.IsMobile)
        {
            var touchState = inputState.CurrentTouchState;
            if (touchState.Count > 0)
            {
                foreach (var touch in touchState)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        TextSelectedCheck(inputState.CurrentCursorLocation);
                    }
                }
            }
        }
        // Handle mouse input for desktop platforms.
        else if (Platformer2DGame.IsDesktop)
        {
            if (inputState.IsLeftMouseButtonClicked())
            {
                TextSelectedCheck(inputState.CurrentCursorLocation);
            }
            else if (inputState.IsMiddleMouseButtonClicked())
            {
                OnSelectEntry(selectedEntry, PlayerIndex.One);
            }
        }

        // Move to the previous menu entry.
        if (inputState.IsMenuUp(ControllingPlayer))
        {
            selectedEntry--;

            if (selectedEntry < 0)
                selectedEntry = menuEntries.Count - 1;

            while (!menuEntries[selectedEntry].Enabled)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }
        }

        // Move to the next menu entry.
        if (inputState.IsMenuDown(ControllingPlayer))
        {
            selectedEntry++;

            if (selectedEntry >= menuEntries.Count)
                selectedEntry = 0;

            SetNextEnabledMenu();
        }

        // Accept or cancel the menu.
        PlayerIndex playerIndex;

        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            OnSelectEntry(selectedEntry, playerIndex);
        }
        else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            OnCancel(playerIndex);
        }
    }

    /// <summary>
    /// Checks if a touch or mouse click has selected a menu entry.
    /// </summary>
    /// <param name="touchLocation">The location of the touch or mouse click.</param>
    private void TextSelectedCheck(Vector2 touchLocation)
    {
        for (int i = 0; i < menuEntries.Count; i++)
        {
            var textSize = ScreenManager.Font.MeasureString(menuEntries[i].Text);
            var entryBounds = new Rectangle((int)menuEntries[i].Position.X, (int)menuEntries[i].Position.Y, (int)textSize.X, (int)textSize.Y);

            if (entryBounds.Contains(touchLocation))
            {
                selectedEntry = i;
                OnSelectEntry(selectedEntry, ControllingPlayer ?? PlayerIndex.One);
                break;
            }
        }
    }

    /// <summary>
    /// Sets the next enabled menu entry as the selected entry.
    /// </summary>
    private void SetNextEnabledMenu()
    {
        while (!menuEntries[selectedEntry].Enabled)
        {
            selectedEntry++;

            if (selectedEntry >= menuEntries.Count)
                selectedEntry = 0;
        }
    }

    /// <summary>
    /// Handler for when the user has chosen a menu entry.
    /// </summary>
    /// <param name="entryIndex">The index of the selected menu entry.</param>
    /// <param name="playerIndex">The index of the player who triggered the selection.</param>
    protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
    {
        menuEntries[entryIndex].OnSelectEntry(playerIndex);
    }

    /// <summary>
    /// Handler for when the user has canceled the menu.
    /// </summary>
    /// <param name="playerIndex">The index of the player who triggered the cancellation.</param>
    protected virtual void OnCancel(PlayerIndex playerIndex)
    {
        ExitScreen();
    }

    /// <summary>
    /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">Event arguments containing the player index.</param>
    protected void OnCancel(object sender, PlayerIndexEventArgs e)
    {
        OnCancel(e.PlayerIndex);
    }

    /// <summary>
    /// Updates the positions of the menu entries. By default, all menu entries
    /// are lined up in a vertical list, centered on the screen.
    /// </summary>
    protected virtual void UpdateMenuEntryLocations()
    {
        // Make the menu slide into place during transitions, using a
        // power curve to make things look more interesting (this makes
        // the movement slow down as it nears the end).
        float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

        // Start at Y = 175; each X value is generated per entry.
        Vector2 position = new Vector2(0f, 175f);

        // Update each menu entry's location in turn.
        for (int i = 0; i < menuEntries.Count; i++)
        {
            MenuEntry menuEntry = menuEntries[i];

            // Each entry is to be centered horizontally.
            position.X = ScreenManager.BaseScreenSize.X / 2 - menuEntry.GetWidth(this) / 2;

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            // Set the entry's position.
            menuEntry.Position = position;

            // Move down for the next entry by the size of this entry.
            position.Y += menuEntry.GetHeight(this);
        }
    }

    /// <summary>
    /// Updates the menu screen.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="otherScreenHasFocus">Whether another screen currently has focus.</param>
    /// <param name="coveredByOtherScreen">Whether this screen is covered by another screen.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        SetNextEnabledMenu();

        // Update each nested MenuEntry object.
        for (int i = 0; i < menuEntries.Count; i++)
        {
            bool isSelected = IsActive && (i == selectedEntry);

            menuEntries[i].Update(this, isSelected, gameTime);
        }
    }

    /// <summary>
    /// Draws the menu screen.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public override void Draw(GameTime gameTime)
    {
        // Make sure our entries are in the right place before we draw them.
        UpdateMenuEntryLocations();

        GraphicsDevice graphics = ScreenManager.GraphicsDevice;
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        // Draw each menu entry in turn.
        for (int i = 0; i < menuEntries.Count; i++)
        {
            MenuEntry menuEntry = menuEntries[i];

            bool isSelected = IsActive && (i == selectedEntry);

            menuEntry.Draw(this, isSelected, gameTime);
        }

        // Make the menu slide into place during transitions, using a
        // power curve to make things look more interesting (this makes
        // the movement slow down as it nears the end).
        float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

        // Draw the menu title centered on the screen.
        Vector2 titlePosition = new Vector2(ScreenManager.BaseScreenSize.X / 2, 80);
        Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
        Color titleColor = menuTitleColor * TransitionAlpha;
        float titleScale = 1.25f;

        titlePosition.Y -= transitionOffset * 100;

        spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                               titleOrigin, titleScale, SpriteEffects.None, 0);

        spriteBatch.End();
    }
}