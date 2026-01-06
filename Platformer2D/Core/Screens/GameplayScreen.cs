using System;
using System.IO;
using Platformer2D.Core;
using Platformer2D.Core.Effects;
using Platformer2D.Core.Inputs;
using Platformer2D.Core.Localization;
using Platformer2D.Core.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace Platformer2D.Screens;

/// <summary>
/// This screen implements the actual game logic and manages the gameplay experience.
/// It controls level loading, player interaction, game state updates, and rendering
/// for the active gameplay session.
/// </summary>
partial class GameplayScreen : GameScreen
{
    /// <summary>
    /// Content manager for loading and managing game assets.
    /// </summary>
    ContentManager content;

    /// <summary>
    /// Controls the opacity of the pause screen overlay when the game is paused.
    /// </summary>
    float pauseAlpha;

    /// <summary>
    /// SpriteBatch instance used for rendering 2D elements.
    /// </summary>
    private SpriteBatch spriteBatch;

    /// <summary>
    /// Current level index (zero-based) in the game progression.
    /// </summary>
    private int levelIndex = 0;

    /// <summary>
    /// Reference to the currently active Level object.
    /// </summary>
    private Level level;

    /// <summary>
    /// Flag indicating if the player has chosen to continue after level completion or failure.
    /// </summary>
    private bool wasContinuePressed;

    /// <summary>
    /// Current state of the gamepad input for the active player.
    /// </summary>
    private GamePadState currentGamePadState;

    /// <summary>
    /// Previous state of the gamepad input for the active player, used to detect button press events.
    /// </summary>
    private GamePadState previousGamePadState;

    /// <summary>
    /// Current state of the keyboard input for the active player.
    /// </summary>
    private KeyboardState currentKeyboardState;

    /// <summary>
    /// Current touch input state, used for mobile device controls.
    /// </summary>
    private TouchCollection currentTouchState;

    /// <summary>
    /// Manager for particle effects in the game.
    /// </summary>
    private ParticleManager particleManager;

    /// <summary>
    /// Manager for leaderboard data and high scores.
    /// </summary>
    private SettingsManager<Platformer2DLeaderboard> leaderboardManager;

    /// <summary>
    /// Text message displayed at the end of a level (completion, failure, etc.).
    /// </summary>
    private string endOfLevelMessage;

    /// <summary>
    /// Tracks the display state of the end-of-level message.
    /// </summary>
    private EndOfLevelMessageState endOfLevelMessgeState;

    /// <summary>
    /// Spacing in pixels between text elements and screen edges.
    /// </summary>
    private const int textEdgeSpacing = 10;

    /// <summary>
    /// Initializes a new instance of the GameplayScreen class.
    /// Sets up transition times for smooth screen changes.
    /// </summary>
    public GameplayScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(1.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    /// <summary>
    /// Loads all content required for the gameplay screen.
    /// This includes loading the initial level, music, and acquiring necessary services.
    /// </summary>
    public override void LoadContent()
    {
        base.LoadContent();

        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        spriteBatch = ScreenManager.SpriteBatch;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(content.Load<Song>("Sounds/Music"));

        particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();

        leaderboardManager ??= ScreenManager.Game.Services.GetService<SettingsManager<Platformer2DLeaderboard>>();

        LoadNextLevel();

        // once the load has finished, we use ResetElapsedTime to tell the game's
        // timing mechanism that we have just finished a very long frame, and that
        // it should not try to catch up.
        ScreenManager.Game.ResetElapsedTime();
    }

    /// <summary>
    /// Loads the next level in sequence, cycling back to the first level after the last one.
    /// Handles level disposal, initialization, and leaderboard setup.
    /// </summary>
    private void LoadNextLevel()
    {
        // move to the next level
        levelIndex = (levelIndex + 1) % Level.NUMBER_OF_LEVELS;

        // Unloads the content for the current level before loading the next one.
        if (level != null)
            level.Dispose();

        // Load the level.
        var levelPath = string.Format("Content/Levels/{0:00}.txt", levelIndex);
        level = new Level(ScreenManager, levelPath, levelIndex);
        level.ParticleManager = particleManager;

        var levelFileName = Path.GetFileName(levelPath);
        var leaderboardFileName = Path.ChangeExtension(levelFileName, ".json");
        leaderboardManager.Storage.SettingsFileName = leaderboardFileName;
        level.LeaderboardManager = leaderboardManager;

        endOfLevelMessgeState = EndOfLevelMessageState.NotShowing;
    }

    /// <summary>
    /// Reloads the current level after a failure.
    /// Decrements the level index and calls LoadNextLevel to reset the current level.
    /// </summary>
    private void ReloadCurrentLevel()
    {
        --levelIndex;
        LoadNextLevel();
    }

    /// <summary>
    /// Unloads all content used by the gameplay screen when it's no longer needed.
    /// </summary>
    public override void UnloadContent()
    {
        content.Unload();
    }

    /// <summary>
    /// Updates the game state, including level logic, end-of-level conditions, and screen transitions.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values for frame-based updates.</param>
    /// <param name="otherScreenHasFocus">Indicates if another screen has focus.</param>
    /// <param name="coveredByOtherScreen">Indicates if this screen is covered by another screen.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                   bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);

        // Gradually fade in or out depending on whether we are covered by the pause screen.
        if (coveredByOtherScreen)
            pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
        else
            pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

        level.Paused = !IsActive;

        if (IsActive)
        {
            if (level.ParticleManager.Finished)
            {
                switch (endOfLevelMessgeState)
                {
                    case EndOfLevelMessageState.NotShowing:
                        if (level.TimeTaken == level.MaximumTimeToCompleteLevel)
                        {
                            if (level.ReachedExit)
                            {
                                endOfLevelMessage = GetLevelStats(Resources.LevelCompleted);
                            }
                            else
                            {
                                endOfLevelMessage = GetLevelStats(Resources.TimeRanOut);
                            }

                            endOfLevelMessgeState = EndOfLevelMessageState.Show;
                        }
                        else if (!level.Player.IsAlive)
                        {
                            endOfLevelMessage = GetLevelStats(Resources.YouDied);
                            endOfLevelMessgeState = EndOfLevelMessageState.Show;
                        }
                        break;
                    case EndOfLevelMessageState.Showing:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Generates a formatted message with level statistics for display at the end of a level.
    /// </summary>
    /// <param name="messageTitle">The title message indicating completion or failure state.</param>
    /// <returns>A formatted string containing the level statistics.</returns>
    private string GetLevelStats(string messageTitle)
    {
        var message = messageTitle + Environment.NewLine + Environment.NewLine;

        if (level.NewHighScore)
            message += Resources.NewHighScore + Environment.NewLine + Environment.NewLine;

        message +=
            Resources.Score + ": " + level.Score + Environment.NewLine +
            Resources.Time + ": " + level.TimeTaken + Environment.NewLine +
            Resources.GemsCollected + $": {level.GemsCollected:D2}/ {level.GemsCount:D2}";

        return message;
    }

    /// <summary>
    /// Processes player input and updates game state accordingly.
    /// Handles pausing, level continuation, and player actions.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="inputState">Current input state for all input devices.</param>
    /// <exception cref="ArgumentNullException">Thrown if inputState is null.</exception>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        ArgumentNullException.ThrowIfNull(inputState);

        base.HandleInput(gameTime, inputState);

        // Get all of our input states for the active player profile.
        int playerIndex = ControllingPlayer != null ? (int)ControllingPlayer.Value : (int)PlayerIndex.One;

        // The game pauses either if the user presses the pause button, or if
        // they unplug the active gamepad. This requires us to keep track of
        // whether a gamepad was ever plugged in, because we don't want to pause
        // on PC if they are playing with a keyboard and have no gamepad at all!
        bool gamePadDisconnected = !currentGamePadState.IsConnected && previousGamePadState.IsConnected;

        Rectangle? backpackTouched = null;
        if (Platformer2DGame.IsMobile)
        {
            backpackTouched = new Rectangle((int)level.BackpackPosition.X,
                (int)level.BackpackPosition.Y,
                (int)level.BackpackPosition.X + 32,
                (int)level.BackpackPosition.Y + 32);
        }

        if (inputState.IsPauseGame(ControllingPlayer, backpackTouched)
                || gamePadDisconnected)
        {
            ScreenManager.AddScreen(new PauseScreen(), ControllingPlayer);
        }
        else
        {
            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime,
                inputState,
                ScreenManager.Game.Window.CurrentOrientation);

            currentKeyboardState = inputState.CurrentKeyboardStates[playerIndex];
            previousGamePadState = inputState.LastGamePadStates[playerIndex];
            currentGamePadState = inputState.CurrentGamePadStates[playerIndex];

            currentTouchState = inputState.CurrentTouchState;

            // Exit the game when back is pressed.
            if (currentGamePadState.Buttons.Back == ButtonState.Pressed)
                ScreenManager.Game.Exit();

            if (endOfLevelMessgeState == EndOfLevelMessageState.Show && IsActive)
            {
                var toastMessageBox = new MessageBoxScreen(endOfLevelMessage, false, new TimeSpan(0, 0, 5), true);
                toastMessageBox.Accepted += (sender, e) =>
                {
                    wasContinuePressed = true;
                };
                endOfLevelMessgeState = EndOfLevelMessageState.Showing;
                ScreenManager.AddScreen(toastMessageBox, ControllingPlayer);
            }

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (wasContinuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeTaken == level.MaximumTimeToCompleteLevel)
                {
                    if (level.ReachedExit)
                    {
                        LoadNextLevel();
                    }
                    else
                    {
                        ReloadCurrentLevel();
                    }
                }

                wasContinuePressed = false;
            }
        }
    }

    /// <summary>
    /// Renders the gameplay elements, including the level, UI, and transition effects.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values for frame-based rendering.</param>
    public override void Draw(GameTime gameTime)
    {
        // This game has a blue background. Why? Because!
        ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0, 0);

        level.Draw(gameTime, spriteBatch);

        base.Draw(gameTime);

        // If the game is transitioning on or off, fade it out to black.
        if (TransitionPosition > 0 || pauseAlpha > 0)
        {
            float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

            ScreenManager.FadeBackBufferToBlack(alpha);
        }
    }
}