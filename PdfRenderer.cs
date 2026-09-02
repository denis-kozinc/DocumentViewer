using Docnet.Core;
using Docnet.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DocumentViewer
{
    public static class PdfRenderer
    {
        // Renderira prvo stran PDF-ja v PNG sliko z belim ozadjem
        // maxDimension = max širina/višina v pikslih (nadzoruje kvaliteto)
        public static void TestRenderFirstPage(byte[] pdfBytes, string outputPath, int maxDimension = 2000)
        {
            // Odpri PDF iz bajtov in pripravi bralnik za prvo stran (index 0)
            using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(maxDimension, maxDimension)))
            using (var pageReader = docReader.GetPageReader(0))
            {
                // Renderiraj stran - dobimo surove pixel podatke (BGRA = modra,zelena,rdeca,alfa/prosojnost)
                var rawBytes = pageReader.GetImage();
                int width = pageReader.GetPageWidth();
                int height = pageReader.GetPageHeight();

                // Pretvori surove bajte v Bitmap objekt (se vedno ima prosojno ozadje)
                using (var transparentBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    var bmpData = transparentBitmap.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format32bppArgb);

                    // Kopiraj surove pixel podatke direktno v bitmap (hitreje kot piksel po piksel)
                    System.Runtime.InteropServices.Marshal.Copy(rawBytes, 0, bmpData.Scan0, rawBytes.Length);
                    transparentBitmap.UnlockBits(bmpData);

                    // Nariši prosojno sliko na belo ozadje, da dobimo koncen izgled (brez crnine)
                    using (var finalBitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb))
                    using (var g = Graphics.FromImage(finalBitmap))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(transparentBitmap, 0, 0); // GDI+ sam poskrbi za blend

                        // Shrani koncen rezultat kot PNG na disk
                        finalBitmap.Save(outputPath, ImageFormat.Png);
                    }
                }
            }
        }

        
    }
}

