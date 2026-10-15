using System.IO;
using System.IO.Pipes;
using System.Text.Json;

namespace DocumentViewer.Pipe
{
    public static class SearchPipe
    {
        public const string Name = "DocumentViewer";

        public static bool TrySend(string[] args)
        {
            try
            {
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
