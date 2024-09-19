namespace NeuralNetwork
{
    using System;
    using System.Collections.Generic;

    public class ConvolutionLayer : Layer
    {
        public List<Volume> Filters { get; set; }

        public List<double> Bias { get; set; }

        public ConvolutionLayer(int numberOfFilters, int filterSize, VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;

            Filters = new List<Volume>();
            Bias = new List<double>(new double[numberOfFilters]);
            for (int i = 0; i < numberOfFilters; i++)
            {
                Filters.Add(Volume.MakeRandom(new VolumeSize(filterSize, filterSize, inputVolumeSize.Z)));
            }

            OutputVolumeSize = new VolumeSize(inputVolumeSize.X  - filterSize + 1, inputVolumeSize.Y - filterSize + 1, numberOfFilters);
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
                var result = Processing.ConvolveWithBias(volume, Filters[i], Bias[i]);
                Buffer.BlockCopy(result.Data, 0, data, offset, result.Data.Length);
                offset += result.Data.Length;
            }

            return new Volume(data, new VolumeSize(volume.Size.X, volume.Size.Y, Filters.Count));
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            // update filters, should just be convolution of filter with error volume layer

            // update bias

            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            if (!error.Size.Equals(OutputVolumeSize))
            {
                throw new ArgumentException("Invalid error size to back propegate");
            }

            var data = new double[volume.Size.X * volume.Size.Y * Filters.Count];
            var offset = 0;
            for (int i = 0; i < Filters.Count; i++)
            {
                var updatedBias = 0.0;
                Volume updatedFilter;
                var result = Processing.Convolve_Backward(volume, Filters[i], error, Bias[i], out updatedFilter, out updatedBias);
                Bias[i] = updatedBias;
                Filters[i] = updatedFilter;
                Buffer.BlockCopy(result.Data, 0, data, offset, result.Data.Length);
                offset += result.Data.Length;
            }

            return new Volume(data, new VolumeSize(volume.Size.X, volume.Size.Y, Filters.Count));
        }

        public override string ToString()
        {
            return "CONV " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
