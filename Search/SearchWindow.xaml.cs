using System.Globalization;
using System.Windows;

namespace DocumentViewer.Search
{
    /// <summary>
    /// Lets the user enter the supported search filters. The fields start filled with the current search.
    /// </summary>
    public partial class SearchWindow : Window
    {
        private const string DateFormat = "yyyy-MM-dd";

        // Set when the user confirms a valid search, null if the window is closed without searching
        public DocumentSearchFilter? Filter { get; private set; }

        public SearchWindow(DocumentSearchFilter current)
        {
            InitializeComponent();
            IssuerBox.Text = current.AcIssuer;
            KeyBox.Text = current.AcKey;
            Doc1Box.Text = current.AcDoc1;
            DeptBox.Text = current.AcDept;
            CostDrvBox.Text = current.AcCostDrv;
            DateFromPicker.SelectedDate = current.DateFrom?.ToDateTime(TimeOnly.MinValue);
            DateToPicker.SelectedDate = current.DateTo?.ToDateTime(TimeOnly.MinValue);
            DocTypeBox.SelectedValue = current.DocType;
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

            if (DocTypeBox.SelectedValue is string docType)
                values["DocType"] = docType;

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
