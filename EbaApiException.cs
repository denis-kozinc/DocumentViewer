using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer
{
    /// <summary>
    /// Represents an exception that occurs when there is an error interacting with the EBA API.
    /// </summary>
    public class EbaApiException(string message) : Exception(message);
}
