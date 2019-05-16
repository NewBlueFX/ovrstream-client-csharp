namespace ovrstream_client_csharp
{
    /// <summary>
    /// The play status of a title.
    /// </summary>
    public enum PlayStatus
    {
        /// <summary>
        /// The title is not visible.
        /// </summary>
        Off,

        /// <summary>
        /// The title is currently rendering.
        /// </summary>
        Rendering,

        /// <summary>
        /// The title is currently paused on screen.
        /// </summary>
        Paused,

        // TODO: Get all available play status here
    }
}
