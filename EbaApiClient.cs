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

        public async Task<byte[]> GetDocumentPdfAsync(string documentId)
        {
            string url = $"{AppConfig.EBA_ENDPOINT}/invoicer/GetDoc?DocId={Uri.EscapeDataString(documentId)}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement result = doc.RootElement.GetProperty("Result");
                EnsureDocSuccess(result);

                string? pdfBase64 = result.GetProperty("pdf").GetString();
                if (string.IsNullOrEmpty(pdfBase64))
                {
                    throw new EbaApiException("Failed to retrieve PDF data.");
                }
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
                JsonElement result = doc.RootElement.GetProperty("Result");
                EnsureListSuccess(result);

                foreach (var element in result.GetProperty("docs").EnumerateArray())
                {
                    idList.Add(element.GetString());
                }
            }
            return idList;
        }

        // Throws when the server reports an error in the body, because it answers with 200 OK even for some errors
        private static void EnsureListSuccess(JsonElement result)
        {
            // The code is written as 200.00000000, so it is read as a double
            if (result.GetProperty("errorCode").GetDouble() == SuccessCode)
                return;

            // errorText is null on success, so the kind is checked before reading it as a string
            JsonElement text = result.GetProperty("erroText");
            string message = text.ValueKind == JsonValueKind.String
                ? text.GetString()!
                : "The server reported an error.";
            throw new EbaApiException(message);
        }

        private static void EnsureDocSuccess(JsonElement result)
        {
            // The code is written as 200.00000000, so it is read as a double
            if (!(result.TryGetProperty("error", out JsonElement error)) || JsonValueKind.Null == error.ValueKind)
                return;
            // A non-string value is unexpected, so it gets the generic message below
            string? message = error.ValueKind == JsonValueKind.String ? error.GetString() : null;
            if (message == string.Empty)
                return;

            // Same as in EnsureListSuccess, the server message is shown as it is
            throw new EbaApiException(message ?? "The server reported an error.");
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
