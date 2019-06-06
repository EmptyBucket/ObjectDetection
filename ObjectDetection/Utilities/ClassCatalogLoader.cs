using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ObjectDetection.Models
{
    public class ClassCatalogLoader
    {
        public async Task Load(string classCatalogUrl, string destinationDirectory)
        {
            var filePath = Path.Combine(destinationDirectory, new Uri(classCatalogUrl).Segments.Last());
            
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(classCatalogUrl, filePath);
        }
    }
}