using System.Threading.Tasks;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Defines methods for saving and loading application settings.
/// </summary>
public interface ISettingsStorage
{
    /// <summary>
    /// Gets or sets the filename used to store settings.
    /// </summary>
    string SettingsFileName { get; set; }

    /// <summary>
    /// Saves the specified settings to storage.
    /// </summary>
    /// <typeparam name="T">The type representing the settings data.</typeparam>
    /// <param name="settings">The settings data to save.</param>
    /// <exception cref="IOException">Thrown if saving fails due to file system issues.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if permission to write the file is denied.</exception>
    void SaveSettings<T>(T settings) where T : new();

    /// <summary>
    /// Loads settings from storage. If no settings exist, it should return <c>null</c> or a default instance.
    /// </summary>
    /// <typeparam name="T">The type representing the settings data.</typeparam>
    /// <returns>The loaded settings or <c>null</c> if no valid settings are found.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the settings file is missing.</exception>
    /// <exception cref="IOException">Thrown if loading fails due to file system issues.</exception>
    T LoadSettings<T>() where T : new();

    /// <summary>
    /// Checks whether the settings file exists in the storage.
    /// </summary>
    /// <returns><c>true</c> if the settings file exists; otherwise, <c>false</c>.</returns>
    bool SettingsExist();
}