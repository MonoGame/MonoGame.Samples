using System;
using Microsoft.Xna.Framework;

namespace Platformer2D.Screens;

/// <summary>
/// Custom event argument which includes the index of the player who
/// triggered the event. This is used by the MenuEntry.Selected event.
/// </summary>
class PlayerIndexEventArgs : EventArgs
{
    PlayerIndex playerIndex;
    /// <summary>
    /// Gets the index of the player who triggered this event.
    /// </summary>
    public PlayerIndex PlayerIndex
    {
        get { return playerIndex; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerIndexEventArgs"/> class.
    /// </summary>
    /// <param name="playerIndex">The player index associated with the event.</param>
    public PlayerIndexEventArgs(PlayerIndex playerIndex)
    {
        this.playerIndex = playerIndex;
    }
}