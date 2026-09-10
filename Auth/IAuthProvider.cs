using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Auth
{
    // Interface for authentication providers
    public interface IAuthProvider
    {
        string GetAuthHeader();
    }
}
