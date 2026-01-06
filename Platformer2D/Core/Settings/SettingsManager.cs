using System;
using System.Diagnostics;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Manages application settings using a specified storage implementation.
/// </summary>
/// <typeparam name="T">The type representing the settings data. Must have a parameterless constructor.</typeparam>
internal class SettingsManager<T> where T : new()
{
    private readonly ISettingsStorage storage;

    /// <summary>
    /// Provides access to the underlying settings storage mechanism.
    /// </summary>
    public ISettingsStorage Storage => storage;

    private T settings;

    /// <summary>
    /// Provides access to the currently loaded settings. Will never be null.
    /// </summary>
    public T Settings => settings;

    /// <summary>
    /// Occurs when settings are successfully loaded.
    /// </summary>
    public event Action<T> SettingsLoaded;

    /// <summary>
    /// Occurs when settings are successfully saved.
    /// </summary>
    public event Action<T> SettingsSaved;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsManager{T}"/> class.
    /// </summary>
    /// <param name="storage">The storage implementation to handle saving and loading settings.</param>
    public SettingsManager(ISettingsStorage storage)
    {
        this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        Load();
    }

    /// <summary>
    /// Saves the current settings to the specified storage.
    /// </summary>
    public void Save()
    {
        try
        {
            storage.SaveSettings(settings);
            SettingsSaved?.Invoke(settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save settings: {ex}");
        }
    }

    /// <summary>
    /// Loads settings from the specified storage. If loading fails, initializes with default values.
    /// </summary>
    public void Load()
    {
        try
        {
            settings = storage.LoadSettings<T>() ?? new T();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load settings, initializing defaults: {ex}");
            settings = new T();
        }

        SettingsLoaded?.Invoke(settings);
    }
}