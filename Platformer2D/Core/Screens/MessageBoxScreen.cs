using System;
using Platformer2D.Core;
using Platformer2D.Core.Inputs;
using Platformer2D.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D.Screens;

/// <summary>
/// A popup message box screen, used to display messages and prompt user input.
/// </summary>
class MessageBoxScreen : GameScreen
{
    private string message;
    private Texture2D gradientTexture;
    private readonly bool toastMessage;
    private readonly TimeSpan toastDuration;
    private TimeSpan toastTimer;
    private Vector2 yesButtonPosition;
    private Vector2 noButtonPosition;
    private Vector2 messageTextPosition;
    private Vector2 yesTextSize;
    private Vector2 noTextSize;
    private Rectangle backgroundRectangle;

    /// <summary>
    /// Event raised when the user accepts the message box.
    /// </summary>
    public event EventHandler<PlayerIndexEventArgs> Accepted;

    /// <summary>
    /// Event raised when the user cancels the message box.
    /// </summary>
    public event EventHandler<PlayerIndexEventArgs> Cancelled;

    // The background includes a border somewhat larger than the text itself.
    private const int hPad = 32;
    private const int vPad = 16;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxScreen"/> class, automatically including usage text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public MessageBoxScreen(string message)
        : this(message, true, TimeSpan.Zero) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxScreen"/> class, allowing customization.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="includeUsageText">Indicates whether to include usage text.</param>
    /// <param name="toastDuration">The duration for toast messages.</param>
    /// <param name="toastMessage">Indicates whether this is a toast message.</param>
    public MessageBoxScreen(string message, bool includeUsageText, TimeSpan toastDuration, bool toastMessage = false)
    {
        string usageText = $"{Environment.NewLine}{Environment.NewLine}{Resources.YesButtonHelp}{Environment.NewLine}{Resources.NoButtonHelp}";

        if (includeUsageText && !toastMessage)
            this.message = message + usageText;
        else
            this.message = message;

        this.toastMessage = toastMessage;
        this.toastDuration = toastDuration;
        this.toastTimer = TimeSpan.Zero;

        IsPopup = true;

        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    /// <summary>
    /// Loads graphics content for this screen.
    /// </summary>
    public override void LoadContent()
    {
        ContentManager content = ScreenManager.Game.Content;

        gradientTexture = content.Load<Texture2D>("Sprites/gradient");
    }

    /// <summary>
    /// Responds to user input, accepting or cancelling the message box.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="inputState">The current input state.</param>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        // Ignore input if this is a ToastMessage
        if (toastMessage)
        {
            return;
        }

        PlayerIndex playerIndex;

        // We pass in our ControllingPlayer, which may either be null (to
        // accept input from any player) or a specific index. If we pass a null
        // controlling player, the InputState helper returns to us which player
        // actually provided the input. We pass that through to our Accepted and
        // Cancelled events, so they can tell which player triggered them.
        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex)
            || (Platformer2DGame.IsMobile
            && inputState.IsUIClicked(new Rectangle((int)yesButtonPosition.X, (int)yesButtonPosition.Y, (int)yesTextSize.X, (int)yesTextSize.Y))))
        {
            // Raise the accepted event, then exit the message box.
            Accepted?.Invoke(this, new PlayerIndexEventArgs(playerIndex));

            ExitScreen();
        }
        else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex)
            || (Platformer2DGame.IsMobile
            && inputState.IsUIClicked(new Rectangle((int)noButtonPosition.X, (int)noButtonPosition.Y, (int)noTextSize.X, (int)noTextSize.Y))))
        {
            // Raise the cancelled event, then exit the message box.
            Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex));

            ExitScreen();
        }
    }

    /// <summary>
    /// Updates the screen, particularly for toast messages and positioning.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="otherScreenHasFocus">Indicates whether another screen has focus.</param>
    /// <param name="coveredByOtherScreen">Indicates whether the screen is covered by another screen.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Handle toast duration countdown.
        if (toastMessage)
        {
            toastTimer += gameTime.ElapsedGameTime;
            if (toastTimer >= toastDuration)
            {
                // Raise the accepted event, then exit the message box.
                Accepted?.Invoke(this, new PlayerIndexEventArgs(PlayerIndex.One));

                // Exit the screen when the toast time has elapsed.
                ExitScreen();
            }
        }

        // Center the message text in the BaseScreenSize.
        // The GlobalTransformation will scale everything for us.
        Vector2 textSize = ScreenManager.Font.MeasureString(message);
        messageTextPosition = (ScreenManager.BaseScreenSize - textSize) / 2;

        // Done here because language setting could change dynamically. Possibly overkill?
        yesTextSize = ScreenManager.Font.MeasureString(Resources.YesButtonText);
        noTextSize = ScreenManager.Font.MeasureString(Resources.NoButtonText);
        if (Platformer2DGame.IsMobile
            && !toastMessage)
        {
            textSize += yesTextSize;
            textSize.Y += vPad * 2;
        }

        backgroundRectangle = new Rectangle((int)messageTextPosition.X - hPad,
                                            (int)messageTextPosition.Y - vPad,
                                            (int)textSize.X + hPad * 2,
                                            (int)textSize.Y + vPad * 2);

        if (Platformer2DGame.IsMobile
            && !toastMessage)
        {
            yesButtonPosition = new Vector2(backgroundRectangle.X + backgroundRectangle.Width - (yesTextSize.X + hPad + noTextSize.X + hPad), backgroundRectangle.Y + backgroundRectangle.Height - yesTextSize.Y - vPad);
            noButtonPosition = new Vector2(backgroundRectangle.X + backgroundRectangle.Width - (noTextSize.X + hPad), backgroundRectangle.Y + backgroundRectangle.Height - noTextSize.Y - vPad);
        }
    }

    /// <summary>
    /// Draws the message box.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        // Darken down any other screens that were drawn beneath the popup.
        ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

        // Fade the popup alpha during transitions.
        Color color = Color.White * TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        // Draw the background rectangle.
        spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

        // Draw the message box text.
        spriteBatch.DrawString(font, message, messageTextPosition, color);

        if (Platformer2DGame.IsMobile
            && !toastMessage)
        {
            color = Color.LimeGreen;
            spriteBatch.DrawString(font, Resources.YesButtonText, yesButtonPosition, color);

            color = Color.OrangeRed;
            spriteBatch.DrawString(font, Resources.NoButtonText, noButtonPosition, color);
        }

        spriteBatch.End();
    }
}