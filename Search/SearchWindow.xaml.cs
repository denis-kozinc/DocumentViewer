using System.Globalization;
using System.Windows;

namespace DocumentViewer.Search
{
    /// <summary>
    /// Lets the user enter the supported search filters. The window always starts empty.
    /// </summary>
    public partial class SearchWindow : Window
    {
        private const string DateFormat = "yyyy-MM-dd";

        // Set when the user confirms a valid search, null if the window is closed without searching
        public DocumentSearchFilter? Filter { get; private set; }

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["acIssuer"] = IssuerBox.Text.Trim(),
                ["acKey"] = KeyBox.Text.Trim(),
                ["acDoc1"] = Doc1Box.Text.Trim(),
                ["acDept"] = DeptBox.Text.Trim(),
                ["acCostDRV"] = CostDrvBox.Text.Trim()
            };

            DateTime? from = DateFromPicker.SelectedDate;
            DateTime? to = DateToPicker.SelectedDate;

            if (from.HasValue != to.HasValue)
            {
                ShowError("Izpolni oba datuma ali nobenega.");
                return;
            }
            if (from.HasValue && to.HasValue)
            {
                if (from > to)
                {
                    ShowError("Datum od je za datumom do.");
                    return;
                }
                values["adDate"] = $"{from.Value.ToString(DateFormat, CultureInfo.InvariantCulture)};{to.Value.ToString(DateFormat, CultureInfo.InvariantCulture)}";
            }

            // Without any criteria the search could match too many documents
            if (values.Values.All(value => value.Length == 0))
            {
                ShowError("Vnesi vsaj en kriterij iskanja.");
                return;
            }

            Filter = SearchArgsParser.Parse(values);
            DialogResult = true;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
