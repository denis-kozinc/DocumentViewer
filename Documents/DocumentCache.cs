using DocumentViewer.Api;
using System.Collections.Concurrent;

namespace DocumentViewer.Documents
{
    /// <summary>
    /// Holds the documents of the current search in memory only
    /// </summary>
    public class DocumentCache
    {
        private readonly EbaApiClient _apiClient;
        private readonly ConcurrentDictionary<string, DocumentContent> _documents = new();
        private const int MaxParallelRequests = 4;

        public DocumentCache(EbaApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Load all documents
        public async Task LoadAllAsync(IReadOnlyList<string> ids, CancellationToken ct)
        {
            _documents.Clear();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxParallelRequests,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(ids, options, async (id, token) =>
            {
                try
                {
                    _documents[id] = await _apiClient.GetDocumentAsync(id, token);
                }
                catch (EbaApiException)
                {
                    // Unknown ID or a document without PDF
                }
            });
        }

        public DocumentContent? Get(string id) => _documents.GetValueOrDefault(id);
    }
}
