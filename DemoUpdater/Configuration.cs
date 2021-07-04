using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoUpdater
{
    static class Configuration
    {
        private static readonly Configurater _configurater = new Configurater();
        private static IConfigurationRoot _configurationRoot => _configurater.ConfigurationRoot;

        public static string GamePath { get => _configurationRoot["GamePath"]; }
        public static string ServerPath { get => _configurationRoot["ServerPath"]; }
        public static string GameVersion { get => _configurationRoot["GameVersion"]; }
        public static string UpdateRoot { get => _configurationRoot["UpdateRoot"]; }
    }
}
