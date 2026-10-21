using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace DocumentViewer.Rendering
{
    /// <summary>
    /// Provides functionality for rendering PDF pages as images.
    /// </summary>
    public static class PdfRenderer
    {
        // helper method to render a page to a Bitmap
        private static Bitmap RenderPageToBitmap(IPageReader pageReader)
        {
            var rawBytes = pageReader.GetImage();
            int width = pageReader.GetPageWidth();
            int height = pageReader.GetPageHeight();

            // convert raw bytes to a Bitmap object (still has transparent background)
            using (var transparentBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                var bmpData = transparentBitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                // copy raw pixel data directly into the bitmap (faster than pixel by pixel)
                System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bmpData.Scan0, rawBytes.Length);
                transparentBitmap.UnlockBits(bmpData);

                // draw the transparent image onto a white background to get the final look (without blackness)
                var finalBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                using (var g = Graphics.FromImage(finalBitmap))
                {
                    g.Clear(Color.White);
                    g.DrawImage(transparentBitmap, 0, 0); // GDI+ automatically handles blending
                }
                // return the final bitmap - caller is responsible for saving/disposing it
                return finalBitmap;
            }
        }

        // renders all pages of a PDF to a list of BitmapSource objects (for WPF);
        // maxDimension is the size in pixels that a page is fitted into (width and height)
        public static List<BitmapSource> RenderAllPages(byte[] pdfBytes, int maxDimension = 2000)
        {
            var pages = new List<BitmapSource>();
            using (var docReader =
                   DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(maxDimension, maxDimension)))
            {
                int pageCount = docReader.GetPageCount();
                for (int i = 0; i < pageCount; i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    using (var bitmap = RenderPageToBitmap(pageReader))
                    {
                        // convert the Bitmap to a BitmapImage for WPF
                        using (var memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, ImageFormat.Png);
                            memoryStream.Position = 0;
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                            pages.Add(bitmapImage);
                        }
                    }
                }
            }
            return pages;
        }

    }
}

