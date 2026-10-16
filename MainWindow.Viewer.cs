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

        private const string AppTitle = "Document Viewer";

        // Space around each page, also needed to work out the fit-to-width zoom
        private const double PageSideMargin = 24;
        private const double PageVerticalMargin = 12;

        // One shared, frozen effect for all pages: frozen objects are immutable, so WPF can reuse them without tracking changes
        private static readonly DropShadowEffect PageShadow = CreatePageShadow();

        // Event handlers for zoom buttons
        private void ZoomOut_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeZoom(-ZoomStep);
        }

        private void ZoomIn_OnClick(object sender, RoutedEventArgs e)
        {
            ChangeZoom(ZoomStep);
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

        // Event handler for scroll changes to update the current page display
        private void PdfScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            int totalPages = PagesPanel.Children.Count;
            if (totalPages == 0) return;

            int currentPage = 1;
            bool atBottom = PdfScrollViewer.ScrollableHeight > 0
                            && e.VerticalOffset >= PdfScrollViewer.ScrollableHeight;
            if (atBottom)
            {
                currentPage = totalPages;
            }
            else
            {
                for (int i = 0; i < totalPages; i++)
                {
                    double top = PagesPanel.Children[i]
                        .TranslatePoint(new Point(0, 0), PdfScrollViewer).Y;
                    if (top > PdfScrollViewer.ViewportHeight / 2) break;
                    currentPage = i + 1;
                }
            }

            CurrentPageText.Text = $"Page {currentPage} of {totalPages}";
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.OemPlus or System.Windows.Input.Key.Add:
                        ChangeZoom(ZoomStep);
                        e.Handled = true;
                        break;
                    case System.Windows.Input.Key.OemMinus or System.Windows.Input.Key.Subtract:
                        ChangeZoom(-ZoomStep);
                        e.Handled = true;
                        break;
                    case System.Windows.Input.Key.D0 or System.Windows.Input.Key.NumPad0:
                        SetZoom(DefaultZoom);
                        e.Handled = true;
                        break;
                }
            }
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
                    Margin = new Thickness(PageSideMargin, PageVerticalMargin, PageSideMargin, PageVerticalMargin),
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
            Title = AppTitle;
            CurrentDocumentText.Text = string.Empty;
            CurrentPageText.Text = string.Empty;
        }

        private static DropShadowEffect CreatePageShadow()
        {
            var shadow = new DropShadowEffect
            {
                BlurRadius = 16,
                ShadowDepth = 1,
                Opacity = 0.25,
                Color = Colors.Black,
            };
            shadow.Freeze();
            return shadow;
        }

        // Zoom so the widest page fits the viewer, never above the default zoom
        private void FitWidth(IReadOnlyList<BitmapSource> pages)
        {
            // The vertical scrollbar is reserved, because it only appears after the content is zoomed
            double available = PdfScrollViewer.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            if (available <= 0) return;

            double pageWidth = pages.Max(p => p.Width) + 2 * PageSideMargin;
            SetZoom(Math.Min(DefaultZoom, available / pageWidth));
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
