using ILGPU.Runtime.Cuda;
using ILGPU.Runtime;
using ILGPU;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System;

namespace NeuralNetwork
{
    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, double, ArrayView<double>> _kernel_ConvolveVolumeWithFilter;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>> _kernel_MaxPool;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>> _kernel_AveragePool;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_FullyConnected;

        static void Kernel_MaxPool(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> result)
        {
            for (int i = 0; i < z; i++)
            {
                var offset = (i * x * y) + (index / 2) * 4 + (index % x) * 2;
                result[((x / 2) * (y / 2) * i) + index] = Math.Max(volume[offset],
                                    Math.Max(volume[offset + 1],
                                        Math.Max(volume[offset + x],
                                                volume[offset + x + 1])));
            }
        }

        static void Kernel_AveragePool(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> result)
        {
            for (int i = 0; i < z; i++)
            {
                var offset = (i * x * y) + (index / 2) * 4 + (index % x) * 2;
                result[((x / 2) * (y / 2) * i) + index] = (volume[offset] + volume[offset + 1] + volume[offset + x] + volume[offset + x + 1]) / 4.0;
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
        static void Kernel_ConvolveVolumeWithFilter(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, double bias, ArrayView<double> result)
        {
            //Convolve(index, volume, filter, x, y, z, filterX, filterY, filterZ, result);
            var volumeIndex = index + x + 1 + (2 * (index / (x - 2)));
            double sum = 0;
            for (int i = 0; i < z; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        var filterIndex = (k + 1) + ((j + 1) * filterX) + (i * filterX * filterY);
                        var n = volumeIndex + k + (x * j) + (x * y * i);
                        sum += volume[n] * filter[filterIndex];
                    }
                }
            }

            result[index] = sum + bias;
        }



        public static Volume ConvolveVolumeWithFilter(Volume volume, Volume filter, double bias)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var filterBuffer = _accelerator.Allocate1D<double>(filter.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            filterBuffer.CopyFromCPU(filter.Data);
            var newSize = new VolumeSize(volume.Size.X - 2, volume.Size.X - 2, volume.Size.Z / filter.Size.Z);
            var resultBuffer = _accelerator.Allocate1D<double>(newSize.TotalSize);
            _kernel_ConvolveVolumeWithFilter(newSize.X * newSize.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, filterBuffer.View, filter.Size.X, filter.Size.Y, filter.Size.Z, bias, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), newSize);
        }

        public static Volume MaxPool(Volume volume)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            var newX = volume.Size.X / 2;
            var newY = volume.Size.Y / 2;
            var resultBuffer = _accelerator.Allocate1D<double>(newX * newY * volume.Size.Z);
            _kernel_MaxPool(newX * newY, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(newX, newY, volume.Size.Z));
        }

        public static Volume AveragePool(Volume volume)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            var newX = volume.Size.X / 2;
            var newY = volume.Size.Y / 2;
            var resultBuffer = _accelerator.Allocate1D<double>(newX * newY * volume.Size.Z);
            _kernel_AveragePool(newX * newY, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, resultBuffer.View);
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

    }
}
