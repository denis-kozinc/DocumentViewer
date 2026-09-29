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

namespace DocumentViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EbaApiClient _apiClient;
        public MainWindow()
        {
            InitializeComponent();

            IAuthProvider authProvider = string.IsNullOrEmpty(AppConfig.API_TOKEN)
                ? new PasswordAuthProvider()
                : new TokenAuthProvider();

            _apiClient = new EbaApiClient(authProvider);

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var pdfBytes = await _apiClient.GetDocumentPdfAsync("ec72894678714b2595dbd17d79882f7b");


            // render all pages and display them
            var pages = PdfRenderer.RenderAllPages(pdfBytes);
            foreach (var page in pages)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 0, 2),
                    //Margin = new Thickness(0,0,0,1),
                    Child = new Image { Source = page },
                };
                PagesPanel.Children.Add(border);
            }
        }

        // Constants for zoom limits
        private const double MinZoom = 0.1;
        private const double MaxZoom = 3.0;

        // Event handlers for zoom buttons
        private void ZoomOut_OnClick(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX = Math.Max(MinZoom, ZoomTransform.ScaleX - 0.1);
            ZoomTransform.ScaleY = Math.Max(MinZoom, ZoomTransform.ScaleY - 0.1);
        }
        private void ZoomIn_OnClick(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX = Math.Min(MaxZoom, ZoomTransform.ScaleX + 0.1);
            ZoomTransform.ScaleY = Math.Min(MaxZoom, ZoomTransform.ScaleY + 0.1);
        }

        // Event handler for mouse wheel zooming
        private void PdfScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                double zoomDelta = e.Delta > 0 ? 0.1 : -0.1;
                double newZoom = ZoomTransform.ScaleX + zoomDelta;

                if (newZoom >= MinZoom && newZoom <= MaxZoom)
                {
                    ZoomTransform.ScaleX = newZoom;
                    ZoomTransform.ScaleY = newZoom;
                }
            }
        }

        // Event handler for scroll changes to update the current page display
        private void PdfScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int totalPages = PagesPanel.Children.Count;
            if (totalPages == 0) return;

            double scrollPosition = e.VerticalOffset;
            double totalHeight = e.ExtentHeight;
            if (totalHeight <= 0) return;

            double singleHeight = totalHeight / totalPages;
            int currentPage = (int)(scrollPosition / singleHeight) + 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            CurrentPageText.Text = $"Page {currentPage} of {totalPages}";
        }
    }
}       