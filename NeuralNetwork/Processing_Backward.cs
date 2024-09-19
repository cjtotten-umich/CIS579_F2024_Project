namespace NeuralNetwork
{
    using ILGPU;
    using ILGPU.Runtime;
    using ILGPU.Runtime.Cuda;
    using System;

    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>, int, int, ArrayView<double>> _kernel_ConvolveVolumeWithFilter_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, ArrayView<double>> _kernel_MaxPool_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, ArrayView<double>> _kernel_AveragePool_Backward;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_FullyConnected_Backward;

        static void Kernel_MaxPool_Backward(Index1D index, ArrayView<double> volume, int errorX, int errorY, int errorZ, ArrayView<double> error, int poolSize, ArrayView<double> result)
        {

            var indexX = index % errorX;
            var indexY = index / errorX;
            for (int z = 0; z < errorZ; z++)
            {
                double max = double.MinValue;
                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (errorX * poolSize)) + (z * (errorX * poolSize) * (errorY * poolSize));
                        if (volume[offset] > max)
                        {
                            max = volume[offset];
                        }
                    }
                }

                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (errorX * poolSize)) + (z * (errorX * poolSize) * (errorY * poolSize));
                        if (volume[offset] == max)
                        {
                            result[offset] = error[index + (z * errorX * errorY)];
                        }
                    }
                }
            }
        }

        static void Kernel_AveragePool_Backward(Index1D index, ArrayView<double> volume, int errorX, int errorY, int errorZ, ArrayView<double> error, int poolSize, ArrayView<double> result)
        {
            var indexX = index % errorX;
            var indexY = index / errorX;
            for (int z = 0; z < errorZ; z++)
            {
                double sum = 0;
                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (errorX * poolSize)) + (z * (errorX * poolSize) * (errorY * poolSize));
                        sum += volume[offset];
                    }
                }

                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (errorX * poolSize)) + (z * (errorX * poolSize) * (errorY * poolSize));
                        var proportion = (volume[offset] / sum);
                        result[offset] = proportion * error[index + (z * errorX * errorY)];
                    }
                }
            }
        }

        static void Kernel_FullyConnected_Backward(Index1D index, ArrayView<double> volume, ArrayView<double> weights, ArrayView<double> bias, ArrayView<double> error, ArrayView<double> result)
        {
            var weightOffset = volume.Length * index;
            bias[index] -= error[index];
            for (int i = 0; i < volume.Length; i++)
            {
                result[i] = weights[weightOffset + i] * error[index];
                weights[weightOffset + i] -= volume[i] * error[index];
            }
        }

        static void Kernel_ConvolveWithBias_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, ArrayView<double> bias, ArrayView<double> error, int errorX, int errorY, ArrayView<double> result)
        {
            bias[0] -= error[index];

            Convolve(index, volume, error, x, y, z, errorX, errorY, z, result);
        }

        public static Volume MaxPool_Backward(Volume volume, Volume error, int poolSize)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            _kernel_MaxPool_Backward(error.Size.X * error.Size.Y, volumeBuffer.View, error.Size.X, error.Size.Y, error.Size.Z, errorBuffer.View, poolSize, resultBuffer.View);
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }

        public static Volume AveragePool_Backward(Volume volume, Volume error, int poolSize)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            _kernel_AveragePool_Backward(error.Size.X * error.Size.Y, volumeBuffer.View, error.Size.X, error.Size.Y, error.Size.Z, errorBuffer.View, poolSize, resultBuffer.View);
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
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            _kernel_FullyConnected_Backward(neurons, volumeBuffer.View, weightBuffer.View, biasBuffer.View, errorBuffer.View, resultBuffer.View);
            updatedBias = new Volume(biasBuffer.GetAsArray1D(), bias.Size);
            updatedWeights = new Volume(weightBuffer.GetAsArray1D(), weights.Size);
            bias.SetData(updatedBias.Data);
            weights.SetData(updatedWeights.Data);
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }
    }
}
