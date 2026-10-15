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
                catch (EbaApiException)
                {
                    // Unknown ID or a document without PDF
                }
            });
            _documents = newDocuments;
        }

        public DocumentContent? Get(string id) => _documents.GetValueOrDefault(id);
    }
}
