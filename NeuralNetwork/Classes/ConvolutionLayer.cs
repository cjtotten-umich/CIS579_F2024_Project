namespace NeuralNetwork
{
    using ILGPU.Runtime.Cuda;
    using System;
    using System.Collections.Generic;

    public class ConvolutionLayer : Layer
    {
        public List<Volume> Filters { get; set; }

        public double[] Bias { get; set; }

        public double LearningRate { get; set; }

        public ConvolutionLayer(int numberOfFilters, int filterSize, double learningRate, VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;

            Filters = new List<Volume>();
            Bias = new double[numberOfFilters];
            for (int i = 0; i < numberOfFilters; i++)
            {
                //Filters.Add(Volume.MakeRandom(new VolumeSize(filterSize, filterSize, inputVolumeSize.Z)));
                Filters.Add(Volume.MakeHeUniform(new VolumeSize(filterSize, filterSize, inputVolumeSize.Z)));
            }

            OutputVolumeSize = new VolumeSize(inputVolumeSize.X  - filterSize + 1, inputVolumeSize.Y - filterSize + 1, numberOfFilters);
            LearningRate = learningRate;
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new FormatException("Input volume is the wrong size");
            }

            var outputVolume = Volume.MakeEmpty(new VolumeSize(OutputVolumeSize.X, OutputVolumeSize.Y, 0));
            for (int i = 0; i < Filters.Count; i++)
            {
                var result = Processing.ConvolveWithBias(volume, Filters[i], Bias[i]);
                outputVolume = outputVolume.Append(result);
            }

            return outputVolume;
        }

        public override Volume BackPropegate(Volume volume, Volume error, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine(this + " - BACKPROP");
            }

            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            if (!error.Size.Equals(OutputVolumeSize))
            {
                throw new ArgumentException("Invalid error size to back propegate");
            }

            // get result to propegate to previous layer
            var result = Volume.MakeZero(InputVolumeSize);
            for (int i = 0; i < Filters.Count; i++)
            {
                var filter = Filters[i];
                var errorSlice = error.Slice(i);
                var paddedErrorLayer = errorSlice.Pad(filter.Size.X + 1);

                var currentError = Volume.MakeEmpty(new VolumeSize(InputVolumeSize.X, InputVolumeSize.Y, 0));
                for (int j = 0; j < filter.Size.Z; j++)
                {
                    var filterSlice = filter.Slice(j);
                    var filterSliceFlip = filterSlice.Flip();
                    var errorLayer = Processing.Convolve_Valid(paddedErrorLayer, filterSliceFlip);
                    currentError = currentError.Append(errorLayer);
                }

                result += currentError;
            }

            // update filter based on gradient of error wrt input
            for (int i = 0; i < Filters.Count; i++)
            {
                var filter = Filters[i];
                var errorSlice = error.Slice(i);
                var gradient = Volume.MakeEmpty(new VolumeSize(filter.Size.X, filter.Size.Y, 0));
                for (int j = 0; j < InputVolumeSize.Z; j++)
                {
                    var inputSlice = volume.Slice(j);
                    var gradientSlice = Processing.Convolve_Valid(inputSlice, errorSlice);
                    gradient = gradient.Append(gradientSlice);
                }

                Filters[i] -= gradient * LearningRate;

                // update bias also based on this error layer
                for (int j = 0; j < errorSlice.Size.TotalSize; j++)
                {
                    Bias[i] -= errorSlice.Data[j] * LearningRate;
                }
            }

            return result;
        }

        public override string ToString()
        {
            return "CONV " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
