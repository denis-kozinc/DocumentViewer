namespace DocumentViewer.Documents
{
    /// <summary>
    /// A row in the document list: the ID used for loading and the text shown to the user.
    /// </summary>
    public record DocumentListItem(string Id, string Label);
}