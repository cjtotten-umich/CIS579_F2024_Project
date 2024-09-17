namespace NeuralNetwork
{
    using System;
    using System.Collections.Generic;

    public class ConvolutionLayer : Layer
    {
        public List<Volume> Filters { get; set; }

        public List<float> Bias { get; set; }

        public ConvolutionLayer(int numberOfFilters, int filterSize, VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;

            Filters = new List<Volume>();
            Bias = new List<float>(new float[numberOfFilters]);
            for (int i = 0; i < numberOfFilters; i++)
            {
                Filters.Add(Volume.MakeRandom(new VolumeSize(filterSize, filterSize, inputVolumeSize.Z)));
            }

            OutputVolumeSize = new VolumeSize(inputVolumeSize.X, inputVolumeSize.Y, numberOfFilters);
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new FormatException("Input volume is the wrong size");
            }

            var data = new double[volume.Size.X * volume.Size.Y * Filters.Count];
            var offset = 0;
            for (int i = 0; i < Filters.Count; i++)
            {
                var result = Processing.Convolve(volume, Filters[i], Bias[i]);
                Buffer.BlockCopy(result.Data, 0, data, offset, result.Data.Length);
                offset += result.Data.Length;
            }

            return new Volume(data, new VolumeSize(volume.Size.X, volume.Size.Y, Filters.Count));
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            // update filters, should just be convolution of filter with error volume layer

            // update bias, 
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "CONV " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
