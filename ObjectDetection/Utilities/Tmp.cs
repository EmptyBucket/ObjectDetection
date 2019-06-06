using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ObjectDetection.Models;
using TensorFlow;

namespace ObjectDetection.Utilities
{
    internal class Programq
    {
        private static readonly ConfigSettings ConfigSettings = new ConfigSettings();
        private static IEnumerable<CatalogItem> catalog;
        private static readonly string CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private const string InputRelative = "test_images/input.jpg";
        private const string OutputRelative = "test_images/output.jpg";
        private static readonly string Input = Path.Combine(CurrentDir, InputRelative);
        private static readonly string Output = Path.Combine(CurrentDir, OutputRelative);
        private static string catalogPath;
        private static string modelPath;

        private const double MinScoreForObjectHighlighting = 0.7;

        private static void Mainq()
        {
            if (catalogPath == null)
            {
                catalogPath = DownloadDefaultTexts(CurrentDir);
            }

            if (modelPath == null)
            {
                modelPath = DownloadDefaultModel(CurrentDir);
            }

            catalog = CatalogUtil.ReadCatalogItems(catalogPath);
            var fileTuples = new List<(string input, string output)> {(Input, Output)};
            var modelFile = modelPath;

            using (var graph = new TFGraph())
            {
                var model = File.ReadAllBytes(modelFile);
                graph.Import(new TFBuffer(model));

                using (var session = new TFSession(graph))
                {
                    Console.WriteLine("Detecting objects");

                    foreach (var tuple in fileTuples)
                    {
                        var tensor = ImageUtil.CreateTensorFromImageFile(tuple.input, TFDataType.UInt8);
                        var runner = session.GetRunner();

                        runner
                            .AddInput(graph["image_tensor"][0], tensor)
                            .Fetch(
                                graph["detection_boxes"][0],
                                graph["detection_scores"][0],
                                graph["detection_classes"][0],
                                graph["num_detections"][0]);
                        var output = runner.Run();

                        var boxes = (float[,,])output[0].GetValue();
                        var scores = (float[,])output[1].GetValue();
                        var classes = (float[,])output[2].GetValue();
                        var num = (float[])output[3].GetValue();

                        DrawBoxes(boxes, scores, classes, tuple.input, tuple.output, MinScoreForObjectHighlighting);
                        Console.WriteLine($"Done. See {OutputRelative}");
                    }
                }
            }
        }

        private static string DownloadDefaultModel(string dir)
        {
            var defaultModelUrl = ConfigSettings.DefaultModelUrl ??
                throw new Exception("'DefaultModelUrl' setting is missing in the configuration file");

            var modelFile = Path.Combine(dir,
                "faster_rcnn_inception_resnet_v2_atrous_coco_2018_01_28/frozen_inference_graph.pb");
            var zipfile = Path.Combine(dir, "faster_rcnn_inception_resnet_v2_atrous_coco_2018_01_28.tar.gz");

            if (File.Exists(modelFile))
            {
                return modelFile;
            }

            if (!File.Exists(zipfile))
            {
                Console.WriteLine("Downloading default model");
                var wc = new WebClient();
                wc.DownloadFile(defaultModelUrl, zipfile);
            }

            ExtractToDirectory(zipfile, dir);
            File.Delete(zipfile);

            return modelFile;
        }

        private static void ExtractToDirectory(string file, string targetDir)
        {
            Console.WriteLine("Extracting");

            using (Stream inStream = File.OpenRead(file))
            using (Stream gzipStream = new GZipInputStream(inStream))
            {
                var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents(targetDir);
            }
        }

        private static string DownloadDefaultTexts(string dir)
        {
            Console.WriteLine("Downloading default label map");

            var defaultTextsUrl = ConfigSettings.DefaultTextsUrl ??
                throw new Exception("'DefaultTextsUrl' setting is missing in the configuration file");
            var textsFile = Path.Combine(dir, "mscoco_label_map.pbtxt");
            var wc = new WebClient();
            wc.DownloadFile(defaultTextsUrl, textsFile);

            return textsFile;
        }

        private static void DrawBoxes(float[,,] boxes, float[,] scores, float[,] classes, string inputFile,
            string outputFile, double minScore)
        {
            var x = boxes.GetLength(0);
            var y = boxes.GetLength(1);
            var z = boxes.GetLength(2);

            float ymin = 0, xmin = 0, ymax = 0, xmax = 0;

            using (var editor = new ImageEditor(inputFile, outputFile))
            {
                for (var i = 0; i < x; i++)
                {
                    for (var j = 0; j < y; j++)
                    {
                        if (scores[i, j] < minScore)
                        {
                            continue;
                        }

                        for (var k = 0; k < z; k++)
                        {
                            var box = boxes[i, j, k];

                            switch (k)
                            {
                                case 0:
                                    ymin = box;
                                    break;
                                case 1:
                                    xmin = box;
                                    break;
                                case 2:
                                    ymax = box;
                                    break;
                                case 3:
                                    xmax = box;
                                    break;
                            }
                        }

                        var value = Convert.ToInt32(classes[i, j]);
                        var catalogItem = catalog.FirstOrDefault(item => item.Id == value);

                        if (catalogItem.DisplayName != "person")
                        {
                            continue;
                        }
                        
                        editor.AddBox(xmin, xmax, ymin, ymax,
                            $"{catalogItem.DisplayName} : {(scores[i, j] * 100).ToString("0")}%");
                    }
                }
            }
        }
    }
}