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

        public DocumentCache(EbaApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // Load all documents
        public async Task LoadAllAsync(IReadOnlyList<string> ids)
        {
            _documents.Clear();

            foreach (string id in ids)
            {
                try
                {
                    _documents[id] = await _apiClient.GetDocumentAsync(id, CancellationToken.None);
                }
                catch (EbaApiException exception)
                {
                    // Unknown ID or a document without PDF
                }
            }
        }

        public DocumentContent? Get(string id) => _documents.GetValueOrDefault(id);
    }
}
