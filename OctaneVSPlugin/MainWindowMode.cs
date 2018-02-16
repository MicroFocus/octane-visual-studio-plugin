namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Represents the modes in which the main window can be in.
    /// </summary>
    public enum MainWindowMode
    {
        /// <summary>
        /// This mode is set on the first use of this plugin and guide the user how to configure the connection to Octane.
        /// </summary>
        FirstTime,

        /// <summary>
        /// This mode is set during loading of the items.
        /// </summary>
        LoadingItems,

        /// <summary>
        /// This mode is set after the items have been loaded.
        /// </summary>
        ItemsLoaded,

        /// <summary>
        /// This mode is set when we fail to load the items usually due to network issue or authentication issues.
        /// </summary>
        FailToLoad
    }
}
