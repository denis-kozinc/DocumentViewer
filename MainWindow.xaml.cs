using System.Formats.Tar;
using DocumentViewer.Auth;
using DocumentViewer.Key;
using DocumentViewer.Search;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DocumentViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EbaApiClient _apiClient;
        private readonly IDocumentKeyProvider _keyProvider;
        private readonly ManualDocumentKeyProvider _manualKeyProvider = new ManualDocumentKeyProvider();
        private readonly DocumentSearchFilter _filter;
        private readonly DocumentCache _documentCache;

        public MainWindow(DocumentSearchFilter filter)
        {
            InitializeComponent();
            _filter = filter;

            // Determine the authentication method based on the presence of an API token
            IAuthProvider authProvider = string.IsNullOrEmpty(AppConfig.API_TOKEN)
                ? new PasswordAuthProvider()
                : new TokenAuthProvider();

            _apiClient = new EbaApiClient(authProvider);
            _documentCache = new DocumentCache(_apiClient);

            _keyProvider = _manualKeyProvider;
            // Subscribe to the DocumentKeyChanged event to update the displayed document when a new key is selected
            _keyProvider.DocumentKeyChanged += (sender, key) =>
            {
                DisplayDocument(key);
                PdfScrollViewer.ScrollToTop();
            };

            Loaded += MainWindow_Loaded;
        }

        // Event handler for the Loaded event of the MainWindow
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowStatus(null);

                IReadOnlyList<string> idList;

                // EbaID names the exact documents, GetList does not know it, so other filters are ignored
                if (_filter.EbaIds.Count > 0)
                    idList = _filter.EbaIds;
                else idList = await _apiClient.SearchDocumentsAsync(_filter);

                if (idList.Count == 0)
                {
                    ShowStatus("No documents found.");
                    return;
                }

                ShowStatus("Loading documents...");
                await _documentCache.LoadAllAsync(idList);
                ShowStatus(null);

                DocumentListBox.ItemsSource = idList;
                // Selecting the first item raises OnSelectionChanged, which displays the document
                DocumentListBox.SelectedIndex = 0;

            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        // Displays the document with the given ID, or shows an error if it is not found or cannot be rendered
        private void DisplayDocument(string documentId)
        {
            var content = _documentCache.Get(documentId);
            if (content == null)
            {
                PagesPanel.Children.Clear();
                CurrentDocumentText.Text = string.Empty;
                ShowStatus("Document not found.");
                return;
            }

            try
            {
                ShowStatus(null);
                // TODO: Add access logging here
                var pages = PdfRenderer.RenderAllPages(content.Pdf);
                ShowPages(pages);
                CurrentDocumentText.Text = $"Document: {content.DisplayName}";
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        // Event handler for document selection changes in the ListBox
        private void DocumentListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocumentListBox.SelectedItem is string selectedId)
            {
                _manualKeyProvider.SelectDocument(selectedId);
            }
        }

        // Shows a message over the viewer, null hides it
        private void ShowStatus(string? message)
        {
            StatusText.Text = message ?? string.Empty;
            StatusText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowError(Exception exception)
        {
            // Only the server error message is shown to the user, other messages could contain sensitive information
            string message = exception switch
            {
                EbaApiException => exception.Message,
                HttpRequestException =>
                    "The server is unreachable or rejected the request. Check the connection and try again.",
                TaskCanceledException => "The server is not responding. Try again.",
                JsonException => "The server returned an unexpected response.",
                FormatException => "The document is invalid.",
                _ => "An unexpected error occurred."
            };
            PagesPanel.Children.Clear();
            CurrentDocumentText.Text = string.Empty;
            ShowStatus(message);
        }
    }
}