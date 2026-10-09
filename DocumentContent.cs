namespace DocumentViewer
{
    /// <summary>
    /// A fetched document: the PDF bytes and the names used to show it in the UI.
    /// </summary>
    public record DocumentContent(string Id, string? InvoiceNumber, string? PantheonId, byte[] Pdf)
    {
        // Invoice number, else the Pantheon ID, else the EbaID
        public string DisplayName => InvoiceNumber ?? PantheonId ?? Id;
    }
}