using System.IO;

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
            // TODO: Consider using a more robust logging framework (e.g., NLog, Serilog) for better log management and formatting.
            File.AppendAllText("access_log.txt", logEntry + Environment.NewLine);
        }
    }
}
