using System;
using System.IO;
using System.Text.Json;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Provides a base implementation for storing and retrieving application settings in JSON format.
/// </summary>
public abstract class BaseSettingsStorage : ISettingsStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettingsStorage"/> class with a default settings file name.
    /// </summary>
    protected BaseSettingsStorage()
    {
        SettingsFileName = "settings.json"; // Default settings file name
    }

    /// <summary>
    /// Specifies the special folder path where settings will be stored.
    /// </summary>
    protected static Environment.SpecialFolder SpecialFolderPath { get; set; }

    private string settingsFileName;

    /// <summary>
    /// Gets or sets the name of the settings file.
    /// </summary>
    public string SettingsFileName
    {
        get => settingsFileName;
        set
        {
            if (settingsFileName != value)
            {
                settingsFileName = value;
            }
        }
    }

    /// <summary>
    /// Gets the full path where the settings file will be stored.
    /// </summary>
    protected string SettingsFilePath => Path.Combine(
        Environment.GetFolderPath(SpecialFolderPath),
        "Platformer2D",
        SettingsFileName);

    /// <summary>
    /// Saves the specified settings object to the designated file path in JSON format.
    /// </summary>
    /// <typeparam name="T">The type of the settings object.</typeparam>
    /// <param name="settings">The settings data to save.</param>
    /// <exception cref="IOException">Thrown if writing to the file fails.</exception>
    public virtual void SaveSettings<T>(T settings) where T : new()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(settings, options);

        // Ensure that the directory exists before writing the file.
        string directoryPath = Path.GetDirectoryName(SettingsFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(SettingsFilePath, jsonString);
    }

    /// <summary>
    /// Loads the settings object from the designated file path.
    /// Returns a new instance of the settings object if no valid file exists.
    /// </summary>
    /// <typeparam name="T">The type of the settings object.</typeparam>
    /// <returns>The loaded settings object or a new instance if loading fails.</returns>
    public virtual T LoadSettings<T>() where T : new()
    {
        if (!SettingsExist())
            return new T();

        string jsonString = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<T>(jsonString) ?? new T();
    }

    /// <summary>
    /// Determines whether the settings file exists at the designated file path.
    /// </summary>
    /// <returns><c>true</c> if the settings file exists; otherwise, <c>false</c>.</returns>
    public bool SettingsExist()
    {
        return !string.IsNullOrEmpty(SettingsFilePath) && File.Exists(SettingsFilePath);
    }
}