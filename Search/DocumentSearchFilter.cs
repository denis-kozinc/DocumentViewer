namespace DocumentViewer.Search
{
    /// <summary>
    /// Holds the optional search criteria for documents. Every field is optional,
    /// an unset field means no filtering on it.
    /// </summary>
    public class DocumentSearchFilter
    {
        public const string DefaultDocType = "racuni";
        public string DocType { get; init; } = DefaultDocType;
        public string? AcIssuer { get; init; }
        public string? AcKey { get; init; }
        public string? AcDoc1 { get; init; }

        // Multiple values are separated by semicolons and passed on as they are
        public string? AcDept { get; init; }
        public string? AcCostDrv { get; init; }

        // The exact documents to show, split on semicolons by the parser. They replace the search, other filters are ignored
        public IReadOnlyList<string> EbaIds { get; init; } = Array.Empty<string>();

        // The API only filters by date when both are set, the parser enforces that
        public DateOnly? DateFrom { get; init; }
        public DateOnly? DateTo { get; init; }
    }
}