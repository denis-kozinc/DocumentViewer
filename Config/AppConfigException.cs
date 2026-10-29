namespace DocumentViewer.Config
{
    /// <summary>
    /// Thrown when the application configuration is missing or invalid.
    /// </summary>
    public class AppConfigException(string message) : Exception(message);
}