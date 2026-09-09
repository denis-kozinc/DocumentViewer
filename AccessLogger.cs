using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocumentViewer
{
    public static class AccessLogger
    {
        // Logs access to a document by appending an entry to a log file
        public static void LogAccess(string documentKey)
        {
            string logEntry = $"{DateTime.Now} | User: {Environment.UserName} | Document: {documentKey}";
            File.AppendAllText("access_log.txt", logEntry + Environment.NewLine);
        }
    }
}
