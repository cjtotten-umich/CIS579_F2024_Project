namespace NeuralNetwork
{
    using ILGPU;
    using ILGPU.Algorithms;
    using ILGPU.IR.Transformations;
    using ILGPU.Runtime;
    using System;
    using System.Numerics;

    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>, int, int, ArrayView<double>> _kernel_ConvolveVolumeWithFilter_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>> _kernel_MaxPool_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>> _kernel_AveragePool_Backward;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_FullyConnected_Backward;

        static void Convolve(Index1D index, ArrayView<double> m1, ArrayView<double> m2, int m1x, int m1y, int m1z , int m2x, int m2y, int m2z, ArrayView<double> result)
        {
            int i = index / (m1x - m2x + 1);
            int j = index % (m1y - m2y + 1);
            double sum = 0;

            for (int m = 0; m < m2x; m++)
            {
                for (int n = 0; n < m2y; n++)
                {
                    for (int d = 0; d < m2z; d++)
                    {
                        int M1Index = ((i + m) * m1y + (j + n)) * m1z + d; // M1 index for 3D access in a 1D array
                        int M2Index = (m * m2y + n) * m2z + d; // M2 index for 3D access in a 1D array

                        // Accumulate the convolution sum
                        sum += m1[M1Index] * m2[M2Index];
                    }
                }
            }

            result[index] = sum;
        }

        static void Kernel_MaxPool_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> error, ArrayView<double> result)
        {
            for (int i = 0; i < z; i++)
            {
                var offset = (i * x * y) + (index / 2) * 4 + (index % x) * 2;
                var v1 = volume[offset];
                var v2 = volume[offset + 1];
                var v3 = volume[offset + x];
                var v4 = volume[offset + x + 1];
                if (v1 >= v2 && v1 >= v3 && v1 >= v4)
                {
                    result[offset] = error[((x / 2) * (y / 2) * i) + index];
                }

                if (v2 >= v1 && v2 >= v3 && v2 >= v4)
                {
                    result[offset + 1] = error[((x / 2) * (y / 2) * i) + index];
                }

                if (v3 >= v1 && v3 >= v2 && v3 >= v4)
                {
                    result[offset + x] = error[((x / 2) * (y / 2) * i) + index];
                }

                if (v4 >= v1 && v4 >= v2 && v4 >= v3)
                {
                    result[offset + x + 1] = error[((x / 2) * (y / 2) * i) + index];
                }
            }
        }

        static void Kernel_AveragePool_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> error, ArrayView<double> result)
        {
            for (int i = 0; i < z; i++)
            {
                var offset = (i * x * y) + (index / 2) * 4 + (index % x) * 2;
                var sum = volume[offset] + volume[offset + 1] + volume[offset + x] + volume[offset + x + 1];
                var v1 = volume[offset] / sum;
                var v2 = volume[offset + 1] / sum;
                var v3 = volume[offset + x] / sum;
                var v4 = volume[offset + x + 1] / sum;
                result[offset] = error[((x / 2) * (y / 2) * i) + index] * v1;
                result[offset + 1] = error[((x / 2) * (y / 2) * i) + index] * v2;
                result[offset + x] = error[((x / 2) * (y / 2) * i) + index] * v3;
                result[offset + x + 1] = error[((x / 2) * (y / 2) * i) + index] * v4;
            }
        }

        static void Kernel_FullyConnected_Backward(Index1D index, ArrayView<double> volume, ArrayView<double> weights, ArrayView<double> bias, ArrayView<double> error, ArrayView<double> result)
        {
            var weightOffset = volume.Length * index;
            bias[index] -= error[index];
            for (int i = 0; i < volume.Length; i++)
            {
                weights[weightOffset + i] -= volume[i] * error[index];
                result[index] += weights[weightOffset + i] * error[index];
            }
        }

        static void Kernel_ConvolveVolumeWithFilter_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, ArrayView<double> bias, ArrayView<double> error, int errorX, int errorY, ArrayView<double> result)
        {
            bias[0] -= error[index];

            var volumeIndex = index + x + (errorX / 2) + (2 * (index / (x - 2)));
            double sum = 0;
            for (int i = 0; i < z; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        var errorIndex = (k + 1) + ((j + 1) * filterX) + (i * filterX * filterY);
                        var n = volumeIndex + k + (x * j) + (x * y * i);

                    }
                }
            }
        }

        public static Volume MaxPool_Backward(Volume volume, Volume error)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            _kernel_MaxPool_Backward(error.Size.X * error.Size.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, errorBuffer.View, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }

        public static Volume AveragePool_Backward(Volume volume, Volume error)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            _kernel_AveragePool_Backward(error.Size.X * error.Size.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, errorBuffer.View, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }

        public static Volume Convolve_Backward(Volume volume, Volume filter, Volume error, double bias, out Volume updatedFilter, out double updatedBias)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var filterBuffer = _accelerator.Allocate1D<double>(filter.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            var biasBuffer = _accelerator.Allocate1D<double>(1);
            volumeBuffer.CopyFromCPU(volume.Data);
            filterBuffer.CopyFromCPU(filter.Data);
            errorBuffer.CopyFromCPU(error.Data);
            biasBuffer.CopyFromCPU(new[] { bias });
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            _kernel_ConvolveVolumeWithFilter_Backward(error.Size.X * error.Size.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, filterBuffer.View, filter.Size.X, filter.Size.Y, filter.Size.Z, biasBuffer.View, errorBuffer.View, error.Size.X, error.Size.Y, resultBuffer.View);
            updatedFilter = new Volume(filterBuffer.GetAsArray1D(), filter.Size);
            updatedBias = biasBuffer.GetAsArray1D()[0];
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }

        public static Volume FullyConnected_Backward(Volume volume, Volume weights, Volume bias, Volume error, out Volume updatedBias, out Volume updatedWeights)
        {
            var neurons = weights.Size.TotalSize / volume.Size.TotalSize;
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var weightBuffer = _accelerator.Allocate1D<double>(weights.Data.Length);
            var biasBuffer = _accelerator.Allocate1D<double>(bias.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            weightBuffer.CopyFromCPU(weights.Data);
            biasBuffer.CopyFromCPU(bias.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(neurons);
            _kernel_FullyConnected_Backward(neurons, volumeBuffer.View, weightBuffer.View, biasBuffer.View, errorBuffer.View, resultBuffer.View);
            updatedBias = new Volume(biasBuffer.GetAsArray1D(), bias.Size);
            updatedWeights = new Volume(weightBuffer.GetAsArray1D(), weights.Size);
            bias.SetData(updatedBias.Data);
            weights.SetData(updatedWeights.Data);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(neurons, 1, 1));
        }
    }
}
