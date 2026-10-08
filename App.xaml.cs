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
                MessageBox.Show(exception.Message, "Invalid arguments", MessageBoxButton.OK, MessageBoxImage.Error);
                // Non-zero exit code, so the caller (Pantheon) can tell the start failed
                Shutdown(1);
                return;
            }

            try
            {
                // Fail early with a clear message, before the window starts calling the API
                AppConfig.Validate();
                new MainWindow(filter).Show();
            }
            catch (AppConfigException exception)
            {
                // The message names the setting but never its value, so it is safe to show
                MessageBox.Show(exception.Message, "Invalid configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
            catch (Exception)
            {
                MessageBox.Show("The application could not be started. Check the configuration.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }
    }

}
