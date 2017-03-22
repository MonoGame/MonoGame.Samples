using StarTrooper2D.Resources;

namespace StarTrooper2D
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        internal AppResources LocalizedResources { get { return _localizedResources; } }
    }
}