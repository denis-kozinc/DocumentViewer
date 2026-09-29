using DocumentViewer.Auth;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace DocumentViewer
{
    /// <summary>
    /// Provides methods to interact with the EBA API to retrieve documents in PDF format.
    /// </summary>
    public class EbaApiClient
    {
        private readonly HttpClient _httpClient;

        public EbaApiClient (IAuthProvider authProvider)
        {
            var handler = new HttpClientHandler();
            // Certificate validation bypassed on purpose, some internal servers dont have valid certificates
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("Authorization", authProvider.GetAuthHeader());
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<string>> QueryContentsAsync(string testno)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/contents?INV_DOC_NUM={testno}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var idList = new List<String>();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                foreach (var element in doc.RootElement.GetProperty("Data").EnumerateArray())
                {
                    idList.Add(element.GetString());
                }
            }
            return idList;
        }
        public async Task<byte[]> GetDocumentPdfAsync(string documentId)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/content/{documentId}?ReturnVisualisation=true";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                string pdfBase64 = doc.RootElement.GetProperty("Pdf").GetString();
                byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
                return pdfBytes;
            }
        }
    }
}
