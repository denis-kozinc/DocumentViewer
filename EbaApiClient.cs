using DocumentViewer.Auth;
using DocumentViewer.Search;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace DocumentViewer
{
    /// <summary>
    /// Provides methods to interact with the EBA API to search for and retrieve documents in PDF format.
    /// </summary>
    public class EbaApiClient
    {
        private readonly HttpClient _httpClient;
        private const string DocType = "racuni";

        private const string DateFormat = "yyyy-MM-dd";

        public EbaApiClient(IAuthProvider authProvider)
        {
            var handler = new HttpClientHandler();
            // Certificate validation bypassed on purpose, some internal servers dont have valid certificates
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("Authorization", authProvider.GetAuthHeader());
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<byte[]> GetDocumentPdfAsync(string documentId)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/invoicer/GetDoc?DocId={Uri.EscapeDataString(documentId)}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                string pdfBase64 = doc.RootElement.GetProperty("Result").GetProperty("pdf").GetString();
                byte[] pdfBytes = Convert.FromBase64String(pdfBase64);
                return pdfBytes;
            }
        }

        public async Task<List<string>> SearchDocumentsAsync(DocumentSearchFilter filter)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/invoicer/GetList?{BuildQueryParameters(filter)}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var idList = new List<string>();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                foreach (var element in doc.RootElement.GetProperty("Result").GetProperty("docs").EnumerateArray())
                {
                    idList.Add(element.GetString());
                }
            }
            return idList;
        }

        // Builds query parameters from the DocumentSearchFilter, skipping unset values
        private static string BuildQueryParameters(DocumentSearchFilter filter)
        {
            var pairs = new List<string>();

            // TODO: Move to config if more document types are needed
            AddPair(pairs, "DocType", DocType);

            AddPair(pairs, "acIssuer", filter.AcIssuer);
            AddPair(pairs, "acKey", filter.AcKey);
            AddPair(pairs, "acDoc1", filter.AcDoc1);
            AddPair(pairs, "acDept", filter.AcDept);
            AddPair(pairs, "acCostDRV", filter.AcCostDrv);
            AddPair(pairs, "Date_from", filter.DateFrom?.ToString(DateFormat, CultureInfo.InvariantCulture));
            // The server treats Date_to as exclusive, so add one day to include the last day
            AddPair(pairs, "Date_to", filter.DateTo?.AddDays(1).ToString(DateFormat, CultureInfo.InvariantCulture));

            return string.Join("&", pairs);
        }

        // Adds "name=value" with an encoded value, skips unset values
        private static void AddPair(List<string> pairs, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            pairs.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}
