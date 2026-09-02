using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DocumentViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // temporary reading of pdf from disk
            byte[] pdfBytes = File.ReadAllBytes(@"C:\Users\denisk\source\repos\DocumentViewer\TestFiles\test.pdf");

            // temporary reading of pdf from disk and rendering first page to png
            PdfRenderer.TestRenderFirstPage(
                pdfBytes,
                @"C:\Users\denisk\source\repos\DocumentViewer\TestFiles\output_one.png"
            );

            // temporary reading of pdf from disk and rendering every page to png
            PdfRenderer.RenderAllPagesToDisk(
                pdfBytes,
                @"C:\Users\denisk\source\repos\DocumentViewer\TestFiles\OutputFolder"
            );
        }
    }
}       