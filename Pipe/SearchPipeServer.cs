using System.IO;
using System.IO.Pipes;
using System.Text.Json;

namespace DocumentViewer.Pipe
{
    /// <summary>
    /// The server side of the named pipe: receives the arguments of later launches and raises SearchRequested.
    /// The event is raised on the thread that called RunAsync (the UI thread), because the awaits keep its context.
    /// </summary>
    public class SearchPipeServer
    {
        private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(5);

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
                    // A client that connects and never sends a line must not block later searches
                    using var readCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    readCts.CancelAfter(ReadTimeout);
                    string? json;
                    try
                    {
                        json = await reader.ReadLineAsync(readCts.Token);
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        continue;
                    }

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
                    // Pause before the next pipe instance, so a persistent failure does not spin the loop
                    try
                    {
                        await Task.Delay(1000, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
    }
}