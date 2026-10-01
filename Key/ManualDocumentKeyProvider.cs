namespace DocumentViewer.Key
{
    public class ManualDocumentKeyProvider : IDocumentKeyProvider
    {
        public event EventHandler<string> DocumentKeyChanged;

        public void SelectDocument(string key)
        {
            DocumentKeyChanged?.Invoke(this, key);
        }
    }
}
