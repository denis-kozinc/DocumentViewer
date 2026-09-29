using System;
using Microsoft.Extensions.Configuration;


namespace DocumentViewer
{
    /// <summary>
    /// Provides access to application configuration values.
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration config;
        static AppConfig()
        {
            config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        }

        public static string EBA_ENDPOINT => config["EBA_ENDPOINT"];
        public static string USER => config["USER"];
        public static string PASS => config["PASS"];
        public static string API_TOKEN => config["API_TOKEN"];
    }

}
