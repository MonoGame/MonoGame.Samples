using Platformer2D.Core.Localization;

namespace Platformer2D.Screens;

/// <summary>
/// Represents the "About" screen, providing information about the game and its technology.
/// This screen displays credits and links to the MonoGame website.
/// </summary>
/// <remarks>
/// This class extends <see cref="MenuScreen"/>, inheriting its menu management capabilities.
/// </remarks>
class AboutScreen : MenuScreen
{
    private MenuEntry builtWithMonoGameMenuEntry;
    private MenuEntry monoGameWebsiteMenuEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutScreen"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets the screen's title and creates the menu entries.
    /// It also hooks up event handlers for menu entry selections.
    /// </remarks>
    public AboutScreen()
        : base(Resources.About) // Assumes Resources.About contains the screen title
    {
        // Create the static label entry. isabled as it's a label
        builtWithMonoGameMenuEntry = new MenuEntry("#BuiltWithMonoGame", false);

        // Create the clickable link entry.
        monoGameWebsiteMenuEntry = new MenuEntry(Resources.MonoGameSite);

        // Create the "Back" button entry.
        MenuEntry back = new MenuEntry(Resources.Back);

        // Attach event handlers for menu entry selections.
        monoGameWebsiteMenuEntry.Selected += MonoGameWebsiteMenuSelected;
        back.Selected += OnCancel;

        // Add the menu entries to the screen.
        MenuEntries.Add(builtWithMonoGameMenuEntry);
        MenuEntries.Add(monoGameWebsiteMenuEntry);
        MenuEntries.Add(back);
    }

    /// <summary>
    /// Handles the selection event for the MonoGame website menu entry.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PlayerIndexEventArgs"/> instance containing the event data.</param>
    private void MonoGameWebsiteMenuSelected(object sender, PlayerIndexEventArgs e)
    {
        LaunchDefaultBrowser("https://www.monogame.net/");
    }

    /// <summary>
    /// Launches the default web browser with the specified URL.
    /// </summary>
    /// <param name="url">The URL to open in the browser.</param>
    /// <remarks>
    /// This method uses <see cref="System.Diagnostics.Process.Start(System.Diagnostics.ProcessStartInfo)"/> to launch the browser.
    /// Note: Platform-specific adjustments might be necessary for cross-platform compatibility.
    /// </remarks>
    private static void LaunchDefaultBrowser(string url)
    {
        // UseShellExecute is crucial for launching the default browser.
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
    }
}