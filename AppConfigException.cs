using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer
{
    /// <summary>
    /// Thrown when the application configuration is missing or invalid.
    /// </summary>
    public class AppConfigException(string message) : Exception(message);
}
