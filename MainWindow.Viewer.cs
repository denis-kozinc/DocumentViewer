using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace DocumentViewer
{
    partial class MainWindow
    {
        // Constants for zoom limits
        private const double MinZoom = 0.1;
        private const double MaxZoom = 3.0;
        private const double ZoomStep = 0.1;
        private const double DefaultZoom = 1.0;

        // One shared, frozen effect for all pages: frozen objects are immutable, so WPF can reuse them without tracking changes
        private static readonly DropShadowEffect PageShadow = CreatePageShadow();

        // Event handlers for zoom buttons
        private void ZoomOut_OnClick(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX = Math.Max(MinZoom, ZoomTransform.ScaleX - ZoomStep);
            ZoomTransform.ScaleY = Math.Max(MinZoom, ZoomTransform.ScaleY - ZoomStep);
        }

        private void ZoomIn_OnClick(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX = Math.Min(MaxZoom, ZoomTransform.ScaleX + ZoomStep);
            ZoomTransform.ScaleY = Math.Min(MaxZoom, ZoomTransform.ScaleY + ZoomStep);
        }

        // Event handler for mouse wheel zooming
        private void PdfScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                ChangeZoom(e.Delta > 0 ? ZoomStep : -ZoomStep);
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

        private void ShowPages(IReadOnlyList<BitmapSource> pages)
        {
            PagesPanel.Children.Clear();
            foreach (var page in pages)
            {
                var image = new Image { Source = page };
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

                var border = new Border
                {
                    Margin = new Thickness(24, 12, 24, 12),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Effect = PageShadow,
                    Child = image,
                };
                PagesPanel.Children.Add(border);
            }
        }

        private void ClearViewer()
        {
            PagesPanel.Children.Clear();
            CurrentDocumentText.Text = string.Empty;
            CurrentPageText.Text = string.Empty;
        }

        private static DropShadowEffect CreatePageShadow()
        {
            var shadow = new DropShadowEffect
            {
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.4,
                Color = Colors.Black,
            };
            shadow.Freeze();
            return shadow;
        }

        private void SetZoom(double zoom)
        {
            zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
            ZoomTransform.ScaleX = zoom;
            ZoomTransform.ScaleY = zoom;
        }

        private void ChangeZoom(double delta)
        {
            SetZoom(ZoomTransform.ScaleX + delta);
        }
    }
}
