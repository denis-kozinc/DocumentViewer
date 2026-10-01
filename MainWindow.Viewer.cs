using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentViewer
{
    partial class MainWindow
    {
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
