using DocumentViewer.Api;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;

namespace DocumentViewer.Documents
{
    /// <summary>
    /// Holds the documents of the current search in memory only
    /// </summary>
    public class DocumentCache
    {
        private readonly EbaApiClient _apiClient;
        private ConcurrentDictionary<string, DocumentContent> _documents = new();
        private const int MaxParallelRequests = 4;

        public DocumentCache(EbaApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Load all documents
        public async Task LoadAllAsync(IReadOnlyList<string> ids, CancellationToken ct)
        {
            _documents = new();
            ConcurrentDictionary<string, DocumentContent> newDocuments = new();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxParallelRequests,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(ids, options, async (id, token) =>
            {
                try
                {
                    newDocuments[id] = await _apiClient.GetDocumentAsync(id, token);
                }
                catch (Exception e) when (e is EbaApiException or FormatException or JsonException
                                          or HttpRequestException { StatusCode: not null })
                {
                    // The server answered with an error for this document only: skip it and load the others.
                    // // No answer at all (connection down, timeout) is not caught here: the others would fail too, so the load stops.
                }
            });
            _documents = newDocuments;
        }

        public DocumentContent? Get(string id) => _documents.GetValueOrDefault(id);
    }
}
