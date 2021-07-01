using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUpdater
{
    class Configuration
    {
        private static string _workerPath;
        public static string GetWorkerPath()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = configurationBuilder.Build();
            _workerPath = configuration["WorkerPath"];
            return _workerPath;
        }
    }
}
