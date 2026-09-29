using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocumentViewer
{
    /// <summary>
    /// Logs access to documents by appending entries to a log file.
    /// </summary>

    // TODO: Not used yet, but will be used in the future to log access to documents.
    public static class AccessLogger
    {
        public static void LogAccess(string documentKey)
        {
            string logEntry = $"{DateTime.Now} | User: {Environment.UserName} | Document: {documentKey}";
            File.AppendAllText("access_log.txt", logEntry + Environment.NewLine);
        }
    }
}
