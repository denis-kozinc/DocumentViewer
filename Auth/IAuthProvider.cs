namespace DocumentViewer.Auth
{
    /// <summary>
    /// Interface for authentication providers.
    /// Implement this interface to provide custom authentication headers for API requests.
    /// </summary>
    public interface IAuthProvider
    {
        string GetAuthHeader();
    }
}