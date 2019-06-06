using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ObjectDetection
{
    public class ConfigSettings
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IConfiguration config;

        public ConfigSettings(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public string ContentRootPath => Path.Combine(hostingEnvironment.ContentRootPath, "www");
        
        public string ModelFileName => config["ModelFileName"];

        public string ClassCatalogFileName => config["ClassCatalogFileName"];
        
        public string ModelUrl => config["ModelUrl"];

        public string ClassCatalogUrl => config["ClassCatalogUrl"];
    }
}