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
            TestStreetView();
            //TestNetwork();
        }

        private static void TestStreetView()
        {
            var distance = 0.01f;
            var step = 0.001f;

            var lat = 42.345036f;
            var lng = -83.289046f;
            var startlat = lat - distance;
            var startlng = lng - distance;

            var threads = new List<Thread>();
            var list = new List<PanoInfo>();    
            for (float currentLat = startlat; currentLat <= lat + distance;  currentLat += step)
            {
                for (float currentLng = startlng; currentLng <  lng + distance; currentLng += step)
                {
                    Console.WriteLine(currentLat + ", " + currentLng);
                    var t = new Thread(thread => list.AddRange(StreetView.GetPanoIds(currentLat, currentLng)));
                    t.Start();
                    threads.Add(t);
                }
            }

            while(threads.Count > 0)
            {
                if (!threads[0].IsAlive)
                {
                    threads.RemoveAt(0);
                    Console.WriteLine(threads.Count.ToString());
                }
            }

            list.Sort((a, b) => a.PanoId.CompareTo(b.PanoId));
            Console.WriteLine("BEFORE:" + list.Count);
            list = list.GroupBy(x => x.PanoId).Select(x => x.First()).ToList();
            Console.WriteLine("AFTER:" + list.Count);

            foreach (var item in list)
            {
                var t = new Thread(thread => StreetView.GetImages(item));
                t.Start();
                threads.Add(t);
            }


            while (threads.Count > 0)
            {
                if (!threads[0].IsAlive)
                {
                    threads.RemoveAt(0);
                    Console.WriteLine(threads.Count.ToString());
                }
            }

            TestNetwork("C:\\Images");
            Console.ReadLine();
        }

        private static void TestNetwork(string path)
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
            

            //for (int i = 0; i < 100; i++)
            //{
            var paths = Directory.GetFiles(path);
            var pathList = new List<string>(paths);
            pathList.Sort((a, b) => a.CompareTo(b));
            foreach (var p in pathList)
            {
                var file = Path.GetFileNameWithoutExtension(p);
                var image = (Bitmap)Image.FromFile(p);
                //var label = File.ReadAllText("C:\\Users\\colin.totten\\Downloads\\Project\\face\\labels\\train\\" + file + ".txt");

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
