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
        public static Random _r = new Random();

        static void Main(string[] args)
        {
            //Console.WriteLine("Enter latitude:");
            //var lat = Console.ReadLine();
            //Console.WriteLine("Enter longitude:");
            //var lng = Console.ReadLine();
            //TestStreetView(Convert.ToDouble(lat), Convert.ToDouble(lng));
            TestNetwork("C:\\Users\\colin.totten\\source\\repos\\NeuralNetwork\\TrainingData\\strictbalanced");
            //TestNetwork("C:\\Users\\colin.totten\\source\\repos\\NeuralNetwork\\TrainingData\\small");
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
            var learningRate = 0.001;
            var r = new Random();
            var model = new Model(512, 512);
            model.AddLayer(new ConvolutionLayer(8, 3, learningRate, new VolumeSize(512, 512, 4)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(510, 510, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(510, 510, 8), 3));
            model.AddLayer(new ConvolutionLayer(8, 3, learningRate, new VolumeSize(170, 170, 8)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(168, 168, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(168, 168, 8), 2));
            model.AddLayer(new ConvolutionLayer(8, 3, learningRate, new VolumeSize(84, 84, 8)));
            model.AddLayer(new ReluActivationLayer(new VolumeSize(82, 82, 8)));
            model.AddLayer(new AveragePoolingLayer(new VolumeSize(82, 82, 8), 2));
            model.AddLayer(new FullyConnectedLayer(50, learningRate, new VolumeSize(41, 41, 8)));
            model.AddLayer(new LayeredNormalizationLayer(new VolumeSize(50, 1, 1)));
            model.AddLayer(new FullyConnectedLayer(3, learningRate, new VolumeSize(50, 1, 1)));
            //model.AddLayer(new SigmoidActivationLayer(new VolumeSize(3, 1, 1)));

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
                var image = (Bitmap)Image.FromFile(folderPath + "\\images\\" + truth[0]);
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
                var image = (Bitmap)Image.FromFile(folderPath + "\\images\\" + truth[0]);
                var probability = Convert.ToDouble(truth[1]);
                var x = Convert.ToDouble(truth[2]);
                var y = Convert.ToDouble(truth[3]);
                var truthVolume = new Volume(new double[] { probability, x, y }, new VolumeSize(3, 1, 1));
                validationItems.Add(new TrainingItem(truth[0], image, truthVolume));
            }

            var epochs = 50;
            
            var history = new List<Tuple<double, double>>();
            for (int i = 0; i < epochs; i++)
            {
                trainingItems = trainingItems.OrderBy(_ => Guid.NewGuid()).ToList();
                for (int j = 0; j < trainingItems.Count; j++)
                {
                    var t = trainingItems[j];
                    Console.WriteLine("TRAINING : " + t.Name + "(" + j + "/" + trainingItems.Count + ") EPOCH:" + i);
                    model.Train(t.Image, t.Truth, false);
                    //var result = model.Process(t.Image);
                    //var probabilityError = t.Truth.Data[0] - result.Data[0];
                    //var xError = t.Truth.Data[1] - result.Data[1];
                    //var yError = t.Truth.Data[2] - result.Data[2];
                    //var distanceError = Math.Sqrt((xError * xError) + (yError * yError));
                    //Console.WriteLine("Probability: " + probabilityError + " Distance:" + distanceError);
                }

                var historyProbability = 0.0;
                var historyDistance = 0.0;
                foreach (var v in validationItems)
                {
                    var error = Processing.StopSignError(model.Process(v.Image), v.Truth);
                     var distanceError = Math.Sqrt((error.Data[1] * error.Data[1]) + (error.Data[2] * error.Data[2]));
                    historyProbability += error.Data[0];
                    historyDistance += distanceError;
                }

                historyProbability /= validationItems.Count;
                historyDistance /= validationItems.Count;
                history.Add(new Tuple<double, double>(historyProbability, historyDistance));
                Console.WriteLine("*****************************************************************");
                Console.WriteLine("Probability: " + historyProbability + " Distance:" + historyDistance);
                Console.WriteLine("*****************************************************************");
            }

            Console.WriteLine("DONE TRAINING");

            using (StreamWriter outputFile = new StreamWriter(folderPath + ".txt"))
            {
                foreach (var h in history)
                {
                    outputFile.WriteLine(h.Item1 + "," + h.Item2);
                }
            }

            Console.WriteLine("Complete, any key to exit...");
            Console.Read();
            //picProcessed.Image = Processing.VolumeToImage(newImage);
        }
    }
}
