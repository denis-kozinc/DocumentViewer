using System.IO;
using System.IO.Pipes;
using System.Text.Json;

namespace DocumentViewer.Pipe
{
    public class SearchPipeServer
    {
        public event Action<string[]>? SearchRequested;

        public async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(SearchPipe.Name, PipeDirection.In,
                        1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                    await server.WaitForConnectionAsync(ct);

                    using var reader = new StreamReader(server);
                    string? json = await reader.ReadLineAsync(ct);
                    if (json == null) continue;

                    string[]? args = JsonSerializer.Deserialize<string[]>(json);
                    if (args != null)
                    {
                        try
                        {
                            SearchRequested?.Invoke(args);
                        }
                        catch (Exception)
                        {
                            // A failing handler must not end the loop, or later searches would never arrive.
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e) when (e is IOException || e is JsonException)
                {
                    try
                    {
                        await Task.Delay(1000, ct);
                    }
                    catch (OperationCanceledException) { break; }
                }
            }
        }
    }
}
