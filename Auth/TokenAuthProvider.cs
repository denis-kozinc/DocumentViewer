namespace DocumentViewer.Auth
{
    /// <summary>
    /// Authentication provider that uses a token for authentication.
    /// It retrieves the token from the application configuration and formats it as a Bearer token for API requests.
    /// </summary>

    public class TokenAuthProvider : IAuthProvider
    {
        public string GetAuthHeader()
        {
            return $"Bearer {AppConfig.API_TOKEN}";
        }
    }
}
