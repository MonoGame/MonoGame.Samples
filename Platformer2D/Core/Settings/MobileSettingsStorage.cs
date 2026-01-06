using System;

namespace Platformer2D.Core.Settings;

/// <summary>
/// Provides a storage mechanism for game settings on mobile platforms.
/// This class inherits from <see cref="BaseSettingsStorage"/> and initializes
/// the storage path to the application's personal folder, as they are most likely to have write access there.
/// </summary>
public class MobileSettingsStorage : BaseSettingsStorage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopSettingsStorage"/> class.
    /// Sets the storage path to the user's personal folder, as they are most likely to have write access there.
    /// </summary>
    public MobileSettingsStorage()
    {
        SpecialFolderPath = Environment.SpecialFolder.Personal;
    }
}