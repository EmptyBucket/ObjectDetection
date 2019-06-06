using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ObjectDetection.Models;
using ObjectDetection.Utilities;
using TensorFlow;

namespace ObjectDetection
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            var configSettings = new ConfigSettings(env);
            
            var modelPatch = Path.Combine(configSettings.ContentRootPath, configSettings.ModelFileName);
            var classCatalogPath = Path.Combine(configSettings.ContentRootPath, configSettings.ClassCatalogFileName);
                
            if (!File.Exists(modelPatch))
            {
                new ModelLoader().Load(configSettings.ModelUrl, configSettings.ContentRootPath).Wait();
            }
                
            if (!File.Exists(classCatalogPath))
            {
                new ClassCatalogLoader().Load(configSettings.ClassCatalogUrl, configSettings.ContentRootPath).Wait();
            }

            var model = File.ReadAllBytes(modelPatch);
            var classCatalogItems = new ClassCatalogParser().Parse(classCatalogPath).ToArray();
            var personClass = classCatalogItems.Single(c => c.DisplayName == "person");

            var graph = new TFGraph();
            lifetime.ApplicationStopped.Register(graph.Dispose);
            graph.Import(new TFBuffer(model));
            
            app.Run(c =>
            {
                 using (var session = new TFSession(graph))
                 {
                     var tensor = TensorUtil.CreateFromImageFile(
                         Path.Combine(configSettings.ContentRootPath, "input.jpg"), TFDataType.UInt8);
                     var output = session
                         .GetRunner()
                         .AddInput(graph["image_tensor"][0], tensor)
                         .Fetch(
                             graph["detection_boxes"][0],
                             graph["detection_scores"][0],
                             graph["detection_classes"][0],
                             graph["num_detections"][0])
                         .Run();
            
                     var boxes = ((float[,,])output[0].GetValue());
                     var scores = (float[,])output[1].GetValue();
                     var classes = (float[,])output[2].GetValue();
                     var num = (float[])output[3].GetValue();
                     
                     classes.GetValue()

                     using (var streamWriter = new StreamWriter(c.Response.Body))
                     {
                         streamWriter.WriteLine($"{{\"result\": \"{num.FirstOrDefault()}\"}}");
                     }

                     c.Response.ContentType = "application/json";
                 }

                 return Task.CompletedTask;
            });
        }
    }
}