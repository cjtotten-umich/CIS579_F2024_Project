using NeuralNetwork;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Drawing;
using StreetViewImageRetrieve;

namespace NeuralNetwork_Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            TestStreetView();
            //TestNetwork();
        }

        private static void TestStreetView()
        {
            StreetView.GetImages(42.345036f, -83.289046f);
        }

        private static void TestNetwork()
        {
            var r = new Random();
            var model = new Model(1280, 720);
            model.AddLayer(new ConvolutionLayer(32, 3, new VolumeSize(1280, 720, 4)));
            model.AddLayer(new MaxPoolingLayer(new VolumeSize(1280, 720, 32)));
            model.AddLayer(new ConvolutionLayer(32, 3, new VolumeSize(640, 360, 32)));
            model.AddLayer(new MaxPoolingLayer(new VolumeSize(640, 360, 32)));
            model.AddLayer(new FullyConnectedLayer(50, new VolumeSize(320, 180, 32)));
            model.AddLayer(new FullyConnectedLayer(4, new VolumeSize(50, 1, 1)));
            model.Build();

            Console.WriteLine(model.ToString());

            //for (int i = 0; i < 100; i++)
            //{
            var paths = Directory.GetFiles("C:\\Users\\colin.totten\\Downloads\\Project\\face\\images\\train");
            var pathList = new List<string>(paths);
            pathList.Sort((a, b) => a.CompareTo(b));
            foreach (var path in pathList)
            {
                var file = Path.GetFileNameWithoutExtension(path);
                var image = (Bitmap)Image.FromFile(path);
                var label = File.ReadAllText("C:\\Users\\colin.totten\\Downloads\\Project\\face\\labels\\train\\" + file + ".txt");

                var sw = Stopwatch.StartNew();
                model.Train(image, Volume.MakeRandom(new VolumeSize(4, 1, 1)));
                sw.Stop();
                //Console.WriteLine(result);
                Console.WriteLine("TIME:" + sw.ElapsedMilliseconds);
                break;
            }
            //}

            Console.WriteLine("Complete, any key to exit...");
            Console.Read();
            //picProcessed.Image = Processing.VolumeToImage(newImage);
        }
    }
}
