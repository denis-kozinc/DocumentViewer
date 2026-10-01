using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using DocumentViewer.Auth;
using DocumentViewer.Key;

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
        public MainWindow()
        {
            InitializeComponent();

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
            // TODO: Replace hardcoded filter with parameters from command-line args (Pantheon integration)
            var idList = await _apiClient.QueryContentsAsync("111");

            DocumentListBox.ItemsSource = idList;

            if (idList.Count > 0)
            {
                _manualKeyProvider.SelectDocument(idList[0]);
            }
        }

        // Method to display the selected document
        private async Task DisplayDocumentAsync(string documentId)
        {
            CurrentDocumentText.Text = $"Document: {documentId}";

            PagesPanel.Children.Clear();
            // Fetch the PDF document by its ID
            var pdfBytes = await _apiClient.GetDocumentPdfAsync(documentId);
            // render all pages and display them
            var pages = PdfRenderer.RenderAllPages(pdfBytes);
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
        }

        // Event handler for document selection changes in the ListBox
        private void DocumentListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocumentListBox.SelectedItem is string selectedId)
            {
                _manualKeyProvider.SelectDocument(selectedId);
            }
        }
    }
}       