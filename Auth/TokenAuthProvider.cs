using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Auth
{
    public class TokenAuthProvider : IAuthProvider
    {
        public string GetAuthHeader()
        {
            return $"Bearer {AppConfig.API_TOKEN}";
        }
    }
}
