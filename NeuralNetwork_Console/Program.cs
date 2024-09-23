using NeuralNetwork;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Drawing;
using StreetViewImageRetrieve;
using System.Threading;
using System.Linq;

namespace NeuralNetwork_Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            TestStreetView(28.597436, -81.244628);
            //TestNetwork("C:\\Images", "C:\\Training");
        }

        private static void TestStreetView(double lat, double lng)
        {
            var sw = Stopwatch.StartNew();

            var distance = 0.01f;
            var step = 0.0005f;
;
            var startlat = lat - distance;
            var startlng = lng - distance;

            var threads = new List<Thread>();
            var list = new List<PanoInfo>();
            Console.WriteLine("Getting Pano IDs");
            for (var currentLat = startlat; currentLat <= lat + distance; currentLat += step)
            {
                for (var currentLng = startlng; currentLng < lng + distance; currentLng += step)
                {
                    var t = new Thread(thread => list.AddRange(StreetView.GetPanoIds(new PanoPosition(currentLat, currentLng))));
                    t.Start();
                    threads.Add(t);
                }
            }

            while (threads.Count > 0)
            {
                if (!threads[0].IsAlive)
                {
                    threads.RemoveAt(0);
                    Console.WriteLine("Waiting for thread completion: " + threads.Count.ToString());
                }
            }

            list.RemoveAll(a => a == null);
            list.Sort((a, b) => a.PanoId.CompareTo(b.PanoId));
            Console.WriteLine("Pano Ids With Duplicates:" + list.Count);
            list = list.GroupBy(x => x.PanoId).Select(x => x.First()).ToList();
            Console.WriteLine("AFTER DEDUPLICATION:" + list.Count);

            foreach (var item in list)
            {
                var t = new Thread(thread => StreetView.GetImages(item));
                t.Start();
                threads.Add(t);
                Console.WriteLine("Launching image download threads:" + threads.Count.ToString());
            }


            while (threads.Count > 0)
            {
                if (!threads[0].IsAlive)
                {
                    threads.RemoveAt(0);
                    Console.WriteLine("Waiting for final thread completion: " + threads.Count.ToString());
                }
            }
            sw.Stop();
            Console.WriteLine("Got all images in " + sw.Elapsed.ToString());
            //TestNetwork("C:\\Images");
            Console.ReadLine();
        }

        private static void TestNetwork(string testImages, string trainingImages)
        {
            var learningRate = 0.01;
            var r = new Random();
            var model = new Model(512, 512);
            model.AddLayer(new ConvolutionLayer(8, 3, 0.01, new VolumeSize(512, 512, 4)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(510, 510, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(510, 510, 8), 3));
            model.AddLayer(new ConvolutionLayer(8, 3, 0.01, new VolumeSize(170, 170, 8)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(168, 168, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(168, 168, 8), 2));
            model.AddLayer(new ConvolutionLayer(8, 3, 0.01, new VolumeSize(84, 84, 8)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(82, 82, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(82, 82, 8), 2));
            model.AddLayer(new FullyConnectedLayer(50, learningRate, new VolumeSize(41, 41, 8)));
            model.AddLayer(new LayeredNormalizationLayer(new VolumeSize(50, 1, 1)));
            model.AddLayer(new FullyConnectedLayer(5, learningRate, new VolumeSize(50, 1, 1)));
            model.AddLayer(new SigmoidActivationLayer(new VolumeSize(5, 1, 1)));

            model.Build();
            
            // Train the model
            var paths = Directory.GetFiles(trainingImages, "*.jpg");
            var pathList = new List<string>(paths);
            var trainingItems = new List<TrainingItem>();
            foreach (var p in pathList)
            {
                var file = Path.GetFileNameWithoutExtension(p);
                var directory = Path.GetDirectoryName(p);
                var image = (Bitmap)Image.FromFile(p);
                var data = File.ReadAllText(directory + "\\" + file + ".txt");
                var truthString = data.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var truth = Array.ConvertAll(truthString, double.Parse);
                var truthVolume = new Volume(truth, new VolumeSize(truth.Length, 1, 1));
                trainingItems.Add(new TrainingItem(file, image, truthVolume));
            }

            var epochs = 50;
            for (int i = 0; i < epochs; i++)
            {
                foreach (var t in trainingItems)
                {
                    Console.WriteLine("TRAINING : " + t.Name + " EPOCH:" + i);
                    model.Train(t.Image, t.Truth, false);
                    var result = model.Process(t.Image);
                    t.LastError = Processing.MeanSquareError(result, t.Truth);
                    Console.WriteLine("AVERAGE ERROR: " + trainingItems.Average(a => a.LastError) + " " + result.StringVersion());
                }
            }

            Console.WriteLine("DONE TRAINING");

            // Test the model
            paths = Directory.GetFiles(testImages);
            pathList = new List<string>(paths);
            pathList.Sort((a, b) => a.CompareTo(b));
            foreach (var p in pathList)
            {
                var image = (Bitmap)Image.FromFile(p);
                var result = model.Process(image);
                if (result.Data[0] > 0.25)
                {
                    Console.WriteLine(p + " - " + result.StringVersion());
                }
            }

            Console.WriteLine("Complete, any key to exit...");
            Console.Read();
            //picProcessed.Image = Processing.VolumeToImage(newImage);
        }
    }
}
