namespace DocumentViewer.Api
{
    /// <summary>
    /// Represents an exception that occurs when there is an error interacting with the EBA API.
    /// </summary>
    public class EbaApiException(string message) : Exception(message);
}