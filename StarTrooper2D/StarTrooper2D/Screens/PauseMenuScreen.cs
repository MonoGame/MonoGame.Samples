#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using GameStateManagement;
#endregion

namespace StarTrooper2D
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseGameScreen : MenuScreen
    {
        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseGameScreen()
            : base("Paused")
        {
            new PauseGameScreen("Paused");

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseGameScreen(string pausereason)
            : base(pausereason)
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;

            EnabledGestures = GestureType.Tap;

            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume Game");
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
        }
        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            // look for any taps that occurred and select any entries that were tapped
            foreach (GestureSample gesture in input.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    ExitScreen();
                }
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            base.Draw(gameTime);
        }


        #endregion
    }
}
