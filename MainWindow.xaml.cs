using DocumentViewer.Api;
using DocumentViewer.Auth;
using DocumentViewer.Config;
using DocumentViewer.Documents;
using DocumentViewer.Key;
using DocumentViewer.Rendering;
using DocumentViewer.Search;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

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
        private DocumentSearchFilter _filter;
        private readonly DocumentCache _documentCache;
        private CancellationTokenSource? _searchCts;
        // The first document shown after startup is zoomed to the window width, later documents keep the zoom
        private bool _fitWidthPending = true;

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
            Closed += (sender, e) => { _searchCts?.Cancel(); _searchCts?.Dispose(); };
        }

        // Event handler for the Loaded event of the MainWindow
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDocumentsAsync(_filter);
        }

        public async Task LoadDocumentsAsync(DocumentSearchFilter filter)
        {
            _filter = filter;
            // A newer search makes the previous load outdated, so it is cancelled
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            // Local copy: the field is replaced by the next call, but this call must keep its own token
            var ct = _searchCts.Token;

            try
            {
                ShowStatus(null);
                RetryButton.Visibility = Visibility.Collapsed;

                // Reset the document list and viewer before starting a new search
                DocumentListBox.ItemsSource = null;
                ClearViewer();
                UpdateNavigationButtons();

                IReadOnlyList<string> idList;

                // EbaID names the exact documents, GetList does not know it, so other filters are ignored
                if (filter.EbaIds.Count > 0)
                    idList = filter.EbaIds;
                else
                    idList = await _apiClient.SearchDocumentsAsync(filter);

                ct.ThrowIfCancellationRequested();

                if (idList.Count == 0)
                {
                    ShowStatus("No documents found.");
                    return;
                }

                ShowStatus("Loading documents...", isBusy: true);
                await _documentCache.LoadAllAsync(idList, ct);
                // The search may have been replaced while this continuation was waiting to run
                ct.ThrowIfCancellationRequested();
                ShowStatus(null);

                var items = idList
                    .Select(id => new DocumentListItem(id, _documentCache.Get(id)?.DisplayName ?? id))
                    .ToList();

                DocumentListBox.ItemsSource = items;
                // Selecting the first item raises OnSelectionChanged, which displays the document
                DocumentListBox.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                if (ct.IsCancellationRequested) return; // The search was cancelled, no need to show an error
                ShowError(exception);
                RetryButton.Visibility = Visibility.Visible;
            }
        }

        // Displays the document with the given ID, or shows an error if it is not found or cannot be rendered
        // synchronous on purpose, because all the documents are already loaded in the cache, so no async is needed
        private void DisplayDocument(string documentId)
        {
            var content = _documentCache.Get(documentId);
            if (content == null)
            {
                ClearViewer();
                ShowStatus("The document could not be loaded.", isError: true);
                return;
            }

            try
            {
                ShowStatus(null);
                // TODO: Add access logging here
                var pages = PdfRenderer.RenderAllPages(content.Pdf);
                ShowPages(pages);
                if (_fitWidthPending)
                {
                    FitWidth(pages);
                    _fitWidthPending = false;
                }
                CurrentDocumentText.Text = content.DisplayName;
                Title = $"{content.DisplayName} - {AppTitle}";
            }
            catch (Exception)
            {
                ClearViewer();
                ShowStatus("The document could not be displayed.", isError: true);
            }
        }

        // Event handler for document selection changes in the ListBox
        private void DocumentListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocumentListBox.SelectedItem is DocumentListItem item)
            {
                _manualKeyProvider.SelectDocument(item.Id);
            }
            UpdateNavigationButtons();
            DocumentListPopup.IsOpen = false;
            PdfScrollViewer.Focus();
        }
        private void DocumentListButton_OnClick(object sender, RoutedEventArgs e)
        {
            DocumentListPopup.IsOpen = true;
        }
        private void DocumentListPopup_OnOpened(object? sender, EventArgs e)
        {
            if (DocumentListBox.SelectedItem != null)
            {
                DocumentListBox.ScrollIntoView(DocumentListBox.SelectedItem);
            }
        }

        // Shows a message over the viewer, null hides it. An error is shown in red, other messages in the default color.
        private void ShowStatus(string? message, bool isError = false, bool isBusy = false)
        {
            bool hasMessage = !string.IsNullOrEmpty(message);
            var brush = isError ? (System.Windows.Media.Brush)FindResource("ErrorBrush") : System.Windows.SystemColors.ControlTextBrush;

            StatusText.Text = message ?? string.Empty;
            StatusText.Foreground = brush;
            StatusText.Visibility = hasMessage ? Visibility.Visible : Visibility.Collapsed;

            // The icon is hidden while busy, the progress ring takes its place
            StatusIcon.Symbol = isError ? Wpf.Ui.Controls.SymbolRegular.ErrorCircle24 : Wpf.Ui.Controls.SymbolRegular.Info24;
            StatusIcon.Foreground = brush;
            StatusIcon.Visibility = hasMessage && !isBusy ? Visibility.Visible : Visibility.Collapsed;
            LoadingRing.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

            StatusCard.Visibility = hasMessage || isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError(Exception exception)
        {
            // Only the server error message is shown to the user, other messages could contain sensitive information
            string message = exception switch
            {
                EbaApiException => $"The server reported: {exception.Message}",
                HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden } =>
                    "Access denied. Check the credentials in the configuration.",
                HttpRequestException { StatusCode: not null } =>
                    "The server returned an error. Try again later.",
                HttpRequestException =>
                    "The server is unreachable. Check the connection and try again.",
                TaskCanceledException => "The server is not responding. Try again.",
                JsonException or FormatException or KeyNotFoundException or InvalidOperationException =>
                    "The server returned an unexpected response.",
                _ => "An unexpected error occurred."
            };
            ClearViewer();
            ShowStatus(message, isError: true);
        }

        private async void RetryButton_OnClick(object sender, RoutedEventArgs e)
        {
            await LoadDocumentsAsync(_filter);
        }
    }
}