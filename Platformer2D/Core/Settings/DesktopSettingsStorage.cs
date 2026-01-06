using System;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Provides a storage mechanism for game settings on desktop platforms.
/// This class inherits from <see cref="BaseSettingsStorage"/> and initializes
/// the storage path to the application's data folder (e.g., AppData on Windows).
/// </summary>
public class DesktopSettingsStorage : BaseSettingsStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopSettingsStorage"/> class.
    /// Sets the storage path to the application's data folder (e.g., AppData on Windows).
    /// </summary>
    public DesktopSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.ApplicationData;
    }
}