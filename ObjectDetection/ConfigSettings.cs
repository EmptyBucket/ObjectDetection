using System.IO;
using Microsoft.Extensions.Configuration;

namespace ObjectDetection
{
    public class ConfigSettings
    {
        private readonly IConfiguration config;

        public ConfigSettings()
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public string DefaultModelUrl => config["ModelUrl"];

        public string DefaultTextsUrl => config["TextsUrl"];
    }
}