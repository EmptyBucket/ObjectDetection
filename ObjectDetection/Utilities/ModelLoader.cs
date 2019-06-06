using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ObjectDetection.Models
{
    public class ModelLoader
    {
        public async Task Load(string modelUrl, string destinationDirectory)
        {
            var modelUri = new Uri(modelUrl);
            var zipPath = Path.Combine(destinationDirectory, modelUri.Segments.Last());
            
            var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(modelUri, zipPath);

            using (var fileStream = File.OpenRead(zipPath))
            using (var gZipInputStream = new GZipInputStream(fileStream))
            {
                TarArchive.CreateInputTarArchive(gZipInputStream).ExtractContents(destinationDirectory);
            }

            File.Delete(zipPath);
        }
    }
}