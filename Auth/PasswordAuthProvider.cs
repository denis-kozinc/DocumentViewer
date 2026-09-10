using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Auth
{
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
