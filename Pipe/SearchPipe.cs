using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DocumentViewer.Pipe
{
    /// <summary>
    /// The client side of the named pipe: a second launch hands its arguments to the running instance.
    /// </summary>
    public static class SearchPipe
    {
        public const string Name = "DocumentViewer";

        private const int AsfwAny = -1;

        // Windows lets only the foreground process bring a window forward; otherwise it just flashes in the taskbar
        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int processId);

        public static bool TrySend(string[] args)
        {
            try
            {
                // This freshly started process holds the foreground right, so it hands it to the running instance
                AllowSetForegroundWindow(AsfwAny);

                using var client = new NamedPipeClientStream(".", Name, PipeDirection.Out, PipeOptions.CurrentUserOnly);

                client.Connect(3000);

                using var writer = new StreamWriter(client) { AutoFlush = true };

                writer.WriteLine(JsonSerializer.Serialize(args));

                return true;
            }
            catch (Exception e) when (e is TimeoutException || e is IOException)
            {
                return false;
            }
        }
    }
}
