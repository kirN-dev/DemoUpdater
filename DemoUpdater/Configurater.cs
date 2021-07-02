using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoUpdater
{
    class Configurater
    {
        public IConfigurationRoot ConfigurationRoot { get; }
        public Configurater()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            ConfigurationRoot = configurationBuilder.Build();
        }
    }
}
