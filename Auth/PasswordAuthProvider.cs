using DocumentViewer.Config;
using System.Text;

namespace DocumentViewer.Auth
{
    /// <summary>
    /// Authentication provider that uses the user name and password from the application configuration
    /// and formats them as a Basic authorization header.
    /// </summary>
    public class PasswordAuthProvider : IAuthProvider
    {
        public string GetAuthHeader()
        {
            string credentials = $"{AppConfig.USER}:{AppConfig.PASS}";
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            string base64Credentials = Convert.ToBase64String(credentialsBytes);

            return $"Basic {base64Credentials}";
        }
    }
}