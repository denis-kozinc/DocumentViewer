using Docnet.Core;
using Docnet.Core.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DocumentViewer.Rendering
{
    /// <summary>
    /// Provides functionality for rendering PDF pages as images.
    /// </summary>
    public static class PdfRenderer
    {
        // The size in layout units that a page is fitted into (width and height), it decides the zoom the viewer starts with
        private const int LayoutDimension = 1600;

        // The size in pixels that a page is rendered into, it decides how sharp the page is
        private const int RenderDimension = 3000;

        // renders all pages of a PDF to a list of BitmapSource objects (for WPF)
        public static List<BitmapSource> RenderAllPages(byte[] pdfBytes)
        {
            var pages = new List<BitmapSource>();
            using (var docReader =
                   DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(RenderDimension, RenderDimension)))
            {
                int pageCount = docReader.GetPageCount();
                for (int i = 0; i < pageCount; i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        // the raw BGRA pixels go straight to WPF, the transparent background is blended with the white
                        // page behind the image in the viewer
                        int width = pageReader.GetPageWidth();
                        int height = pageReader.GetPageHeight();
                        // WPF sizes the image from its DPI, so the extra pixels do not enlarge the page in the viewer
                        double dpi = 96.0 * RenderDimension / LayoutDimension;
                        var page = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null,
                            pageReader.GetImage(), width * 4);
                        page.Freeze();
                        pages.Add(page);
                    }
                }
            }

            return pages;
        }
    }
}