using DocumentViewer.Auth;
using DocumentViewer.Key;
using DocumentViewer.Search;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        public MainWindow(DocumentSearchFilter filter)
        {
            InitializeComponent();
            _filter = filter;

            // Determine the authentication method based on the presence of an API token
            IAuthProvider authProvider = string.IsNullOrEmpty(AppConfig.API_TOKEN)
                ? new PasswordAuthProvider()
                : new TokenAuthProvider();

            _apiClient = new EbaApiClient(authProvider);

            _keyProvider = _manualKeyProvider;
            // Subscribe to the DocumentKeyChanged event to update the displayed document when a new key is selected
            _keyProvider.DocumentKeyChanged += async (sender, key) =>
            {
                await DisplayDocumentAsync(key);
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

                if (_filter.AcKey != null)
                {
                    await DisplayDocumentAsync(_filter.AcKey);
                    return;
                }

                var idList = await _apiClient.SearchDocumentsAsync(_filter);

                if (idList.Count == 0)
                {
                    ShowStatus("No documents found.");
                    return;
                }

                DocumentListBox.ItemsSource = idList;
                // Selecting the first item raises OnSelectionChanged, which displays the document
                DocumentListBox.SelectedIndex = 0;

            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        // Method to display the selected document
        private async Task DisplayDocumentAsync(string documentId)
        {
            CurrentDocumentText.Text = $"Loading: {documentId}";

            try
            {
                ShowStatus(null);

                // Fetch the PDF document by its ID
                var pdfBytes = await _apiClient.GetDocumentPdfAsync(documentId);
                // TODO: Add access logging here

                // render all pages and display them
                var pages = PdfRenderer.RenderAllPages(pdfBytes);
                PagesPanel.Children.Clear();
                foreach (var page in pages)
                {
                    var border = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0, 0, 0, 2),
                        Child = new Image { Source = page },
                    };
                    PagesPanel.Children.Add(border);
                }
                CurrentDocumentText.Text = $"Document: {documentId}";
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