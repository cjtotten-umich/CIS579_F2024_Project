using ILGPU.Runtime.Cuda;
using ILGPU.Runtime;
using ILGPU;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using ILGPU.IR.Analyses;

namespace NeuralNetwork
{
    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, double, ArrayView<double>> _kernel_ConvolveWithBias;
        static Action<Index1D, ArrayView<double>, int, int, int, int, ArrayView<double>> _kernel_MaxPool;
        static Action<Index1D, ArrayView<double>, int, int, int, int, ArrayView<double>> _kernel_AveragePool;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_FullyConnected;
        static Action<Index1D, ArrayView<double>, int, int, ArrayView<double>> _kernel_LayeredNormalization;
        

        static void Kernel_LayeredNormalization(Index1D index, ArrayView<double> volume, int x, int y, ArrayView<double> result)
        {
            var offset = index * x * y;
            var average = 0.0;
            for (int i = 0; i < x * y; i++)
            {
                average += volume[offset + i];
            }

            average /= x * y;

            var variance = 0.0;
            for (int i = 0; i < x * y; i++)
            {
                variance += Math.Pow(volume[offset + i] - average, 2) ;
            }

            variance /= x * y;
            variance = Math.Sqrt(variance + 0.0000000001);

            for (int i = 0; i < x * y; i++)
            {
                result[offset + i] = (volume[offset + i] - average)/ variance;
            }
        }

        static void Kernel_MaxPool(Index1D index, ArrayView<double> volume, int outputX, int outputY, int outputZ, int poolSize, ArrayView<double> result)
        {
            var indexX = index % outputX;
            var indexY = index / outputX;
            for (int z = 0; z < outputZ; z++)
            {
                double max = double.MinValue;
                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (outputX * poolSize)) + (z * (outputX * poolSize) * (outputY * poolSize));
                        if (volume[offset] > max)
                        {
                            max = volume[offset];
                        }
                    }
                }

                result[index + (z * outputX * outputY)] = max;
            }
        }

        static void Kernel_AveragePool(Index1D index, ArrayView<double> volume, int outputX, int outputY, int outputZ, int poolSize, ArrayView<double> result)
        {
            var indexX = index % outputX;
            var indexY = index / outputX;
            for (int z = 0; z < outputZ; z++)
            {
                double sum = 0;
                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (outputX * poolSize)) + (z * (outputX * poolSize) * (outputY * poolSize));
                        sum += volume[offset];
                    }
                }

                result[index + (z * outputX * outputY)] = sum / (poolSize * poolSize);
            }
        }

        static void Kernel_FullyConnected(Index1D index, ArrayView<double> volume, ArrayView<double> weights, ArrayView<double> bias, ArrayView<double> result)
        {
            var sum = 0.0;
            var weightOffset = volume.Length * index;
            for (int i = 0; i < volume.Length; i++)
            {
                sum += weights[weightOffset + i] * volume[i];
            }

            result[index] = sum + bias[index];
        }
        static void Kernel_ConvolveWithBias(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, double bias, ArrayView<double> result)
        {
            Kernel_ConvolveValid(index.X, volume, filter, x, y, z, filterX, filterY, filterZ, result);
            result[index] += bias;
        }

        public static Volume ConvolveWithBias(Volume volume, Volume filter, double bias)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var filterBuffer = _accelerator.Allocate1D<double>(filter.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            filterBuffer.CopyFromCPU(filter.Data);
            var newSize = new VolumeSize(volume.Size.X - filter.Size.X + 1, volume.Size.Y - filter.Size.Y + 1, 1);
            var resultBuffer = _accelerator.Allocate1D<double>(newSize.TotalSize);
            _kernel_ConvolveWithBias(newSize.X * newSize.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, filterBuffer.View, filter.Size.X, filter.Size.Y, filter.Size.Z, bias, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), newSize);
        }

        public static Volume MaxPool(Volume volume, int poolSize)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            var newX = volume.Size.X / poolSize;
            var newY = volume.Size.Y / poolSize;
            var resultBuffer = _accelerator.Allocate1D<double>(newX * newY * volume.Size.Z);
            _kernel_MaxPool(newX * newY, volumeBuffer.View, newX, newY, volume.Size.Z, poolSize, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(newX, newY, volume.Size.Z));
        }

        public static Volume AveragePool(Volume volume, int poolSize)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            var newX = volume.Size.X / poolSize;
            var newY = volume.Size.Y / poolSize;
            var resultBuffer = _accelerator.Allocate1D<double>(newX * newY * volume.Size.Z);
            _kernel_AveragePool(newX * newY, volumeBuffer.View, newX, newY, volume.Size.Z, poolSize, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(newX, newY, volume.Size.Z));
        }

        public static Volume FullyConnected(Volume volume, Volume weights, Volume bias)
        {
            var neurons = weights.Size.TotalSize / volume.Size.TotalSize;
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var weightBuffer = _accelerator.Allocate1D<double>(weights.Data.Length);
            var biasBuffer = _accelerator.Allocate1D<double>(bias.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            weightBuffer.CopyFromCPU(weights.Data);
            biasBuffer.CopyFromCPU(bias.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(neurons);
            _kernel_FullyConnected(neurons, volumeBuffer.View, weightBuffer.View, biasBuffer.View, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(neurons, 1, 1));
        }

        public static Volume LayeredNormalization(Volume volume)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
                {
                    _kernel_LayeredNormalization(volume.Size.Z, volumeBuffer.View, volume.Size.X, volume.Size.Y, resultBuffer.View);
                    return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
                }
            }
        }
    }
}
