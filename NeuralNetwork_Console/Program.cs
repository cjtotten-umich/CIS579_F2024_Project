using NeuralNetwork;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Drawing;
using StreetViewImageRetrieve;
using System.Threading;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace NeuralNetwork_Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Enter latitude:");
            //var lat = Console.ReadLine();
            //Console.WriteLine("Enter longitude:");
            //var lng = Console.ReadLine();
            //TestStreetView(Convert.ToDouble(lat), Convert.ToDouble(lng));
            TestNetwork("C:\\Users\\colin.totten\\source\\repos\\NeuralNetwork\\TrainingData\\small");
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

        private static void TestNetwork(string folderPath)
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
            model.AddLayer(new FullyConnectedLayer(3, learningRate, new VolumeSize(50, 1, 1)));
            model.AddLayer(new SigmoidActivationLayer(new VolumeSize(3, 1, 1)));

            model.Build();

            // Train the model
            var trainingItems = new List<TrainingItem>();
            var data = File.ReadAllLines(folderPath + "\\train.csv");
            foreach (var line in data)
            {
                if (line.Contains("filename"))
                {
                    continue;
                }

                var truth = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var image = (Bitmap)System.Drawing.Image.FromFile(folderPath + "\\images\\" + truth[0]);
                var probability = Convert.ToDouble(truth[1]);
                var x = Convert.ToDouble(truth[2]);
                var y = Convert.ToDouble(truth[3]);
                var truthVolume = new Volume(new double[] { probability, x, y }, new VolumeSize(3, 1, 1));
                trainingItems.Add(new TrainingItem(truth[0], image, truthVolume));
            }

            // Load validation
            var validationItems = new List<TrainingItem>();
            data = File.ReadAllLines(folderPath + "\\validate.csv");
            foreach (var line in data)
            {
                if (line.Contains("filename"))
                {
                    continue;
                }

                var truth = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var image = (Bitmap)System.Drawing.Image.FromFile(folderPath + "\\images\\" + truth[0]);
                var probability = Convert.ToDouble(truth[1]);
                var x = Convert.ToDouble(truth[2]);
                var y = Convert.ToDouble(truth[3]);
                var truthVolume = new Volume(new double[] { probability, x, y }, new VolumeSize(3, 1, 1));
                validationItems.Add(new TrainingItem(truth[0], image, truthVolume));
            }

            var epochs = 50;
            for (int i = 0; i < epochs; i++)
            {
                for (int j = 0; j < trainingItems.Count; j++)
                {
                    var t = trainingItems[j];
                    Console.WriteLine("TRAINING : " + t.Name + "(" + j + "/" + trainingItems.Count + ") EPOCH:" + i);
                    model.Train(t.Image, t.Truth, false);
                    var result = model.Process(t.Image);
                    t.LastError = Processing.MeanSquareError(result, t.Truth);
                    Console.WriteLine("AVERAGE ERROR: " + trainingItems.Average(a => a.LastError) + " " + result.StringVersion());
                }

                foreach(var v in validationItems)
                {
                    var result = model.Process(v.Image);
                    Console.WriteLine(v.Truth.Data[0] - result.Data[0]);
                }
            }

            Console.WriteLine("DONE TRAINING");


            Console.WriteLine("Complete, any key to exit...");
            Console.Read();
            //picProcessed.Image = Processing.VolumeToImage(newImage);
        }
    }
}
