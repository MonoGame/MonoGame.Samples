using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Represents the leaderboard data for the game, including the fastest completion time
/// and the number of gems collected. This class implements <see cref="INotifyPropertyChanged"/>
/// to notify subscribers when a property value changes, enabling data binding and UI updates.
/// </summary>
internal class Platformer2DLeaderboard : INotifyPropertyChanged
{
    // TODO: Add PlayerName property in the future.
    // public string PlayerName { get; set; }

    /// <summary>
    /// Gets or sets the fastest time taken to complete the game.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the fastest completion time.
    /// </value>
    public TimeSpan FastestTime { get; set; }

    /// <summary>
    /// Gets or sets the total number of gems collected in the game.
    /// </summary>
    /// <value>
    /// An integer representing the number of gems collected.
    /// </value>
    public int GemsCollected { get; set; }

    /// <summary>
    /// Event triggered when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event to notify subscribers that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. If not provided, the name of the calling member is used.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}