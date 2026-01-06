using System.ComponentModel;
using System.Runtime.CompilerServices;
using Platformer2D.Core.Effects;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Represents the game settings, including display, language, and particle effects.
/// This class implements <see cref="INotifyPropertyChanged"/> to notify subscribers
/// when a property value changes, enabling data binding and UI updates.
/// </summary>
public class Platformer2DSettings : INotifyPropertyChanged
{
    private bool fullScreen;
    private int language = 2; // Default to English for now
    private ParticleEffectType particleEffect;

    /// <summary>
    /// Gets or sets whether the game is in full-screen mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if the game is in full-screen mode; otherwise, <c>false</c>.
    /// </value>
    public bool FullScreen
    {
        get => fullScreen;
        set
        {
            if (fullScreen != value)
            {
                fullScreen = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the language setting for the game.
    /// </summary>
    /// <value>
    /// An integer representing the selected language. The value corresponds to a language
    /// option in the game's localization system.
    /// </value>
    public int Language
    {
        get => language;
        set
        {
            if (language != value)
            {
                language = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the type of particle effect used in the game.
    /// </summary>
    /// <value>
    /// A <see cref="ParticleEffectType"/> value representing the current particle effect.
    /// </value>
    public ParticleEffectType ParticleEffect
    {
        get => particleEffect;
        set
        {
            if (particleEffect != value)
            {
                particleEffect = value;
                OnPropertyChanged();
            }
        }
    }

    // Add more settings as needed

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