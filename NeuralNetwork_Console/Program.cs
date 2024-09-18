using NeuralNetwork;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Drawing;
using StreetViewImageRetrieve;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace NeuralNetwork_Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            //TestStreetView(42.30854f, -83.12639f);
            TestNetwork("C:\\Images", "C:\\Training");
        }

        private static void TestStreetView(float lat, float lng)
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
            for (float currentLat = startlat; currentLat <= lat + distance; currentLat += step)
            {
                for (float currentLng = startlng; currentLng < lng + distance; currentLng += step)
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
            var r = new Random();
            var model = new Model(512, 512);
            model.AddLayer(new ConvolutionLayer(32, 3, new VolumeSize(512, 512, 4)));
            model.AddLayer(new MaxPoolingLayer(new VolumeSize(512, 512, 32)));
            model.AddLayer(new ConvolutionLayer(32, 3, new VolumeSize(256, 256, 32)));
            model.AddLayer(new MaxPoolingLayer(new VolumeSize(256, 256, 32)));
            model.AddLayer(new FullyConnectedLayer(50, new VolumeSize(128, 128, 32)));
            model.AddLayer(new FullyConnectedLayer(4, new VolumeSize(50, 1, 1)));
            model.Build();
            
            // Train the model
            var paths = Directory.GetFiles(trainingImages);
            var pathList = new List<string>(paths);
            pathList.Sort((a, b) => a.CompareTo(b));
            foreach (var p in pathList)
            {
                var file = Path.GetFileNameWithoutExtension(p);
                var directory = Path.GetDirectoryName(p);
                var image = (Bitmap)Image.FromFile(p);
                var data = File.ReadAllText(directory + file + ".txt");
                var truthString = data.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var truth = Array.ConvertAll(truthString, double.Parse);
                model.Train(image, new Volume(truth, new VolumeSize(truth.Length, 1, 1)));
                break;
            }

            // Test the model
            paths = Directory.GetFiles(trainingImages);
            pathList = new List<string>(paths);
            pathList.Sort((a, b) => a.CompareTo(b));
            foreach (var p in pathList)
            {
                var file = Path.GetFileNameWithoutExtension(p);
                var directory = Path.GetDirectoryName(p);
                var image = (Bitmap)Image.FromFile(p);
                var data = File.ReadAllText(directory + file + ".txt");
                var truthString = data.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var truth = Array.ConvertAll(truthString, double.Parse);
                var result = model.Process(image);

                break;
            }

            Console.WriteLine("Complete, any key to exit...");
            Console.Read();
            //picProcessed.Image = Processing.VolumeToImage(newImage);
        }
    }
}
