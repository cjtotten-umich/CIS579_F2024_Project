namespace NeuralNetwork
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Text;

    public class Model
    {
        public Layer FirstLayer { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public bool IsBuilt { get; private set; }

        public Model(int imageWidth, int imageHeight) 
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
        }

        public void Build()
        {
            var currentLayer = FirstLayer;
            while (currentLayer.NextLayer != null)
            {
                currentLayer.PreviousLayer = currentLayer;
                currentLayer = currentLayer.NextLayer;
            }

            IsBuilt = true;
        }

        public Volume Process (Bitmap image)
        {
            if (!IsBuilt)
            {
                throw new NotSupportedException("Model not built");
            }

            var resizedImage = new Bitmap(image, ImageWidth, ImageHeight);
            var volume = Processing.ImageToVolume(resizedImage);
            var currentLayer = FirstLayer;
            while (currentLayer.NextLayer != null)
            {
                volume = currentLayer.Process(volume);
                currentLayer = currentLayer.NextLayer;
            }

            return currentLayer.Process(volume);
        }

        public void Train(Bitmap image, Volume truth, bool verbose)
        {
            if (!IsBuilt)
            {
                throw new NotSupportedException("Model not built");
            }

            var resizedImage = new Bitmap(image, ImageWidth, ImageHeight);
            var volume = Processing.ImageToVolume(resizedImage);
            FirstLayer.Train(volume, truth, verbose);
        }

        public void AddLayer(Layer layer)
        {
            if (FirstLayer == null)
            {
                FirstLayer = layer;
                FirstLayer.PreviousLayer = null;
            }
            else
            {
                var currentLayer = FirstLayer;
                while (currentLayer.NextLayer != null)
                {
                    currentLayer = currentLayer.NextLayer;
                }

                currentLayer.NextLayer = layer;
                layer.PreviousLayer = currentLayer;
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            var currentLayer = FirstLayer;
            while (currentLayer.NextLayer != null)
            {
                b.Append(currentLayer.ToString() + " => ");
                currentLayer = currentLayer.NextLayer;
            }

            b.Append(currentLayer.ToString());
            return b.ToString();
        }
    }
}
