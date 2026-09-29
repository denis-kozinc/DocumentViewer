using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Auth
{
    // TODO: Comes later, but will be used in the future to access documents with username and password authentication.
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
