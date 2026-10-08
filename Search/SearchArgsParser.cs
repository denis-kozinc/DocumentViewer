using System.Globalization;

namespace DocumentViewer.Search
{
    /// <summary>
    /// Provides functionality for parsing search arguments from the command line.
    /// </summary>
    public static class SearchArgsParser
    {
        private const string Prefix = "--";

        // TODO: Confirm the date format and the separator of adDate
        private const string DateFormat = "yyyy-MM-dd";
        private static readonly char[] DateSeparators = { ';', ',' };

        // The known argument names, case-insensitive
        private static readonly HashSet<string> KnownNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "acIssuer", "acKey", "acDoc1", "acDept", "acCostDRV", "EbaID", "adDate"
        };

        // Parses the command-line arguments into a DocumentSearchFilter object
        public static DocumentSearchFilter Parse(string[] args)
        {
            var values = ReadPairs(args);
            var (dateFrom, dateTo) = ParseDateRange(values);

            return new DocumentSearchFilter
            {
                AcKey = GetValue(values, "acKey"),
                AcIssuer = GetValue(values, "acIssuer"),
                AcDoc1 = GetValue(values, "acDoc1"),
                AcDept = GetValue(values, "acDept"),
                AcCostDrv = GetValue(values, "acCostDRV"),
                EbaId = GetValue(values, "EbaID"),
                DateFrom = dateFrom,
                DateTo = dateTo
            };
        }

        // Reads the command-line arguments as pairs of name and value, returning a dictionary
        private static Dictionary<string, string> ReadPairs(string[] args)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i += 2)
            {
                if (!args[i].StartsWith(Prefix, StringComparison.Ordinal))
                    throw new ArgumentException($"Expected an argument name starting with '{Prefix}'.");

                string name = args[i].Substring(Prefix.Length);

                if (!KnownNames.Contains(name))
                    throw new ArgumentException($"Unknown argument '{Prefix}{name}'.");

                bool valueMissing = i + 1 >= args.Length
                                    || args[i + 1].StartsWith(Prefix, StringComparison.Ordinal);
                if (valueMissing)
                    throw new ArgumentException($"Argument '{Prefix}{name}' has no value.");

                if (values.ContainsKey(name))
                    throw new ArgumentException($"Argument '{Prefix}{name}' is given more than once.");

                values[name] = args[i + 1].Trim();
            }

            return values;
        }

        // An empty value counts as not set, so a caller can pass all arguments and leave some blank
        private static string? GetValue(Dictionary<string, string> values, string name)
        {
            if (values.TryGetValue(name, out string? value))
            {
                if (value.Length > 0)
                    return value;
            }
            return null;
        }

        // adDate is one value holding two dates (from, to), a single date is not valid
        private static (DateOnly? From, DateOnly? To) ParseDateRange(Dictionary<string, string> values)
        {
            string? text = GetValue(values, "adDate");
            if (text == null)
                return (null, null);

            string[] parts = text.Split(DateSeparators);
            if (parts.Length != 2)
                throw new ArgumentException($"Argument '{Prefix}adDate' must contain exactly two dates (from, to).");

            DateOnly from = ParseDate(parts[0].Trim());
            DateOnly to = ParseDate(parts[1].Trim());

            if (from > to)
                throw new ArgumentException($"Argument '{Prefix}adDate' has the start date after the end date.");

            return (from, to);
        }

        // Parses a single date string into a DateOnly object, throwing an exception if the format is invalid
        private static DateOnly ParseDate(string text)
        {
            if (!DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateOnly date))
                throw new ArgumentException($"Argument '{Prefix}adDate' contains an invalid date.");

            return date;
        }
    }
}
