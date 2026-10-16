using System.Windows;

namespace DocumentViewer
{
    partial class MainWindow
    {
        private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DocumentListBox.SelectedIndex > 0)
            {
                DocumentListBox.SelectedIndex--;
            }
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (DocumentListBox.SelectedIndex < DocumentListBox.Items.Count - 1)
            {
                DocumentListBox.SelectedIndex++;
            }
        }

        private void UpdateNavigationButtons()
        {
            var index = DocumentListBox.SelectedIndex;
            PreviousButton.IsEnabled = index > 0;
            NextButton.IsEnabled = index >= 0 && index < DocumentListBox.Items.Count - 1;
            FirstButton.IsEnabled = index > 0;
            LastButton.IsEnabled = index >= 0 && index < DocumentListBox.Items.Count - 1;
        }

        private void FirstButton_OnClick(object sender, RoutedEventArgs e)
        {
            DocumentListBox.SelectedIndex = 0;
        }

        private void LastButton_OnClick(object sender, RoutedEventArgs e)
        {
            DocumentListBox.SelectedIndex = DocumentListBox.Items.Count - 1;
        }
    }
}
