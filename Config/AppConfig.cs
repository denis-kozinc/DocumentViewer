using Microsoft.Extensions.Configuration;


namespace DocumentViewer.Config
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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AppConfig).Assembly, optional: true, reloadOnChange: false)
                .Build();
        }

        // Given by the caller (Ares) at startup, they take priority over appsettings and user secrets
        // (which stay for development), and they live in memory only
        private static string? _endpointOverride;
        private static string? _tokenOverride;

        public static string EBA_ENDPOINT => (_endpointOverride ?? Read("EBA_ENDPOINT")).TrimEnd('/');
        public static string USER => Read("USER");
        public static string PASS => Read("PASS");
        public static string API_TOKEN => _tokenOverride ?? Read("API_TOKEN");

        // Sets the endpoint and token given on the command line, empty values are ignored.
        public static void Override(string? endpoint, string? token)
        {
            if (!string.IsNullOrEmpty(endpoint))
                _endpointOverride = endpoint;
            if (!string.IsNullOrEmpty(token))
                _tokenOverride = token;
        }

        /// <summary>
        /// Checks the configuration at startup, so a mistake shows up as a clear message
        /// instead of a failing API call later. Messages never contain the values.
        /// </summary>
        public static void Validate()
        {
            bool endpointIsHttps = Uri.TryCreate(EBA_ENDPOINT, UriKind.Absolute, out Uri? endpoint)
                                   && endpoint.Scheme == Uri.UriSchemeHttps;
            if (!endpointIsHttps)
                throw new AppConfigException("The endpoint is missing or is not an https:// address.");

            bool hasToken = !string.IsNullOrEmpty(API_TOKEN);
            bool hasPassword = !string.IsNullOrEmpty(USER) && !string.IsNullOrEmpty(PASS);
            if (!hasToken && !hasPassword)
                throw new AppConfigException("The access token is missing.");
        }

        private static string Read(string key) => (_config[key] ?? string.Empty).Trim();
    }

}
