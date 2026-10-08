using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentViewer.Search
{
    /// <summary>
    /// Holds the optional search criteria for documents. Every field is optional,
    /// an unset field means no filtering on it.
    /// </summary>
    public class DocumentSearchFilter
    {
        public string? AcIssuer { get; init; }
        public string? AcKey { get; init; }
        public string? AcDoc1 { get; init; }

        // Multiple values are separated by semicolons and passed on as they are
        public string? AcDept { get; init; }
        public string? AcCostDrv { get; init; }
        public string? EbaId { get; init; }

        // The API only filters by date when both are set, the parser enforces that
        public DateOnly? DateFrom { get; init; }
        public DateOnly? DateTo { get; init; }

        // True when the Pantheon key is set, so the PDF can be fetched directly without a list.
        public bool HasKey => !string.IsNullOrWhiteSpace(AcKey);
    }
}
