using System.Windows;
using DocumentViewer.Search;

namespace DocumentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DocumentSearchFilter filter;
            try
            {
                filter = SearchArgsParser.Parse(e.Args);
            }
            catch (ArgumentException exception)
            {
                // TODO: Replace with proper UI messages (error handling checkpoint)
                MessageBox.Show(exception.Message, "Invalid arguments", MessageBoxButton.OK, MessageBoxImage.Error);
                // Non-zero exit code, so the caller (Pantheon) can tell the start failed
                Shutdown(1);
                return;
            }

            new MainWindow(filter).Show();
        }
    }

}
