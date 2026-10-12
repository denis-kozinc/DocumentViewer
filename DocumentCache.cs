using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DocumentViewer
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
        public async Task LoadAllAsync(IReadOnlyList<string> ids)
        {
            _documents.Clear();

            var options = new ParallelOptions { MaxDegreeOfParallelism = MaxParallelRequests };

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
