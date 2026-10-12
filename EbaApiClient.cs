using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
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
        private const string DateFormat = "yyyy-MM-dd";
        private const int SuccessCode = 200;

        public EbaApiClient(IAuthProvider authProvider)
        {
            var handler = new HttpClientHandler();
            // Certificate validation bypassed on purpose, some internal servers dont have valid certificates
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("Authorization", authProvider.GetAuthHeader());
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<string>> SearchDocumentsAsync(DocumentSearchFilter filter)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/invoicer/GetList?{BuildQueryParameters(filter)}";

            // TODO: Remove this debug line in production, it may contain sensitive information
            Debug.WriteLine(url);

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var idList = new List<string>();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement result = doc.RootElement.GetProperty("Result");
                EnsureListSuccess(result);

                foreach (var element in result.GetProperty("docs").EnumerateArray())
                {
                    if (element.GetString() is string id)
                        idList.Add(id);
                }
            }
            return idList;
        }

        public async Task<DocumentContent> GetDocumentAsync(string documentId, CancellationToken token)
        {
            // The generic API answers on this path for any DocType
            string url = $"{AppConfig.EBA_ENDPOINT}/ebanext/racuni/content/{Uri.EscapeDataString(documentId)}?ReturnVisualisation=true";

            using var response = await _httpClient.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(token);

            // The server answers an unknown ID with an empty 200 body
            if (string.IsNullOrWhiteSpace(json))
                throw new EbaApiException("Document not found.");

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            string? pdfBase64 = root.TryGetProperty("Pdf", out JsonElement pdfElement)
                                && pdfElement.ValueKind == JsonValueKind.String
                ? pdfElement.GetString()
                : null;
            if (string.IsNullOrEmpty(pdfBase64))
                throw new EbaApiException("The document has no PDF.");
            
            // Data may be missing, an undefined element is then not an object and every field reads as null
            JsonElement data = root.TryGetProperty("Data", out JsonElement dataElement) ? dataElement : default;

            return new DocumentContent(
                documentId,
                ReadString(data, "INV_DOC_NUM"),
                ReadString(root, "ExternId"),
                Convert.FromBase64String(pdfBase64)
            );
        }

        // Returns the trimmed value of a plain string field of the given object, or null when it is missing, empty or not a string
        private static string? ReadString(JsonElement parent, string name)
        {
            if (parent.ValueKind == JsonValueKind.Object
                && parent.TryGetProperty(name, out JsonElement field)
                && field.ValueKind == JsonValueKind.String)
            {
                string trimmed = field.GetString()!.Trim();
                return trimmed.Length > 0 ? trimmed : null;
            }
            return null;
        }

        // Throws when the server reports an error in the body, because it answers with 200 OK even for some errors
        private static void EnsureListSuccess(JsonElement result)
        {
            // The code is written as 200.00000000, so it is read as a double
            if (result.GetProperty("errorCode").GetDouble() == SuccessCode)
                return;

            // errorText is null on success, so the kind is checked before reading it as a string
            string message = "The server reported an error.";
            if (result.TryGetProperty("errorText", out JsonElement text) && text.ValueKind == JsonValueKind.String)
            {
                message = text.GetString()!;
            }
            throw new EbaApiException(message);
        }

        // Builds query parameters from the DocumentSearchFilter, skipping unset values
        private static string BuildQueryParameters(DocumentSearchFilter filter)
        {
            var pairs = new List<string>();

            AddPair(pairs, "DocType", filter.DocType);
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
