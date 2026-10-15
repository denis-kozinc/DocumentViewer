using DocumentViewer.Config;
using DocumentViewer.Pipe;
using DocumentViewer.Search;
using System.Windows;

namespace DocumentViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private SearchPipeServer? _pipeServer;
        private static Mutex? _mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure only one instance of the application is running
            _mutex = new Mutex(true, @"Local\DocumentViewer", out bool isFirst);
            if (!isFirst)
            {
                _mutex.Dispose();
                _mutex = null;

                bool success = SearchPipe.TrySend(e.Args);
                Shutdown(success ? 0 : 1);
                return;
            }

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
                _cancellationTokenSource = new CancellationTokenSource();
                _pipeServer = new SearchPipeServer();
                _pipeServer.SearchRequested += (args) =>
                {
                    MessageBox.Show(string.Join(" ", args));
                };
                new MainWindow(filter).Show();
                _ = _pipeServer.RunAsync(_cancellationTokenSource.Token);
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

        protected override void OnExit(ExitEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }

}
