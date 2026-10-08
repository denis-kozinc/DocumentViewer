using Microsoft.Extensions.Configuration;


namespace DocumentViewer
{
    /// <summary>
    /// Provides access to application configuration values.
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration _config;
        static AppConfig()
        {
            // reloadOnChange is off on purpose: values are read once at startup
            // (the auth header is built once too), so reloading would change nothing
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }

        public static string EBA_ENDPOINT => Read("EBA_ENDPOINT").TrimEnd('/');
        public static string USER => Read("USER");
        public static string PASS => Read("PASS");
        public static string API_TOKEN => Read("API_TOKEN");

        /// <summary>
        /// Checks the configuration at startup, so a mistake shows up as a clear message
        /// instead of a failing API call later. Messages never contain the values.
        /// </summary>
        public static void Validate()
        {
            bool endpointIsHttps = Uri.TryCreate(EBA_ENDPOINT, UriKind.Absolute, out Uri? endpoint)
                                   && endpoint.Scheme == Uri.UriSchemeHttps;
            if (!endpointIsHttps)
                throw new AppConfigException("EBA_ENDPOINT must be set to an https:// address.");

            // Same rule as in MainWindow: a token wins, otherwise user and password are used
            bool hasToken = !string.IsNullOrEmpty(API_TOKEN);
            bool hasPassword = !string.IsNullOrEmpty(USER) && !string.IsNullOrEmpty(PASS);
            if (!hasToken && !hasPassword)
                throw new AppConfigException("Set API_TOKEN, or both USER and PASS.");
        }

        private static string Read(string key) => (_config[key] ?? string.Empty).Trim();
    }

}
