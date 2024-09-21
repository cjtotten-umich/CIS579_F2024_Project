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
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>, double> _kernel_FullyConnected_Backward;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, int, int, int, ArrayView<double>> _kernel_FullyConnected_Backward_InputGradient;
        static Action<Index1D, ArrayView<double>, int, int, ArrayView<double>, ArrayView<double>> _kernel_LayeredNormalization_Backward;
        
        static void Kernel_LayeredNormalization_Backward(Index1D index, ArrayView<double> volume, int x, int y, ArrayView<double> error, ArrayView<double> result)
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
                variance += Math.Pow(volume[offset + i] - average, 2);
            }

            variance /= x * y;
            variance = Math.Sqrt(variance + 0.0000000001);

            for (int i = 0; i < x * y; i++)
            {
                result[offset + i] = (error[offset + i] * variance) + average;
            }
        }

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
                        else
                        {
                            result[offset] = 0;
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
                for (int y = 0; y < poolSize; y++)
                {
                    for (int x = 0; x < poolSize; x++)
                    {
                        var offset = x + (indexX * poolSize) + (((indexY * poolSize) + y) * (errorX * poolSize)) + (z * (errorX * poolSize) * (errorY * poolSize));
                        result[offset] = error[index + (z * errorX * errorY)] / (poolSize * poolSize);
                    }
                }
            }
        }

        static void Kernel_FullyConnected_Backward(Index1D index, ArrayView<double> volume, ArrayView<double> weights, ArrayView<double> bias, ArrayView<double> error, double learningRate)
        {
            var weightOffset = volume.Length * index;
            bias[index] -= (learningRate * error[index]);
            for (int i = 0; i < volume.Length; i++)
            {
                weights[weightOffset + i] -= volume[i] * (learningRate * error[index]);
            }
        }

        static void Kernel_FullyConnected_Backward_InputGradient(Index1D index, ArrayView<double> weights, ArrayView<double> error, int neurons, int x, int y, ArrayView<double> result)
        {
            for (int i = 0; i < neurons; i++)
            {
                result[index] += weights[(x * y * i) + index] * error[i];
            }
        }

        static void Kernel_ConvolveWithBias_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, ArrayView<double> bias, ArrayView<double> error, int errorX, int errorY, ArrayView<double> result)
        {
            bias[0] -= error[index];
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

        public static Volume ConvolveWithBias_Backward(Volume volume, Volume filter, Volume error, double bias, out Volume updatedFilter, out double updatedBias)
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

        public static Volume Convolve_Valid(Volume v1, Volume v2)
        {
            var v1Buffer = _accelerator.Allocate1D<double>(v1.Data.Length);
            var v2Buffer = _accelerator.Allocate1D<double>(v2.Data.Length);
            v1Buffer.CopyFromCPU(v1.Data);
            v2Buffer.CopyFromCPU(v2.Data);
            int resultX = v1.Size.X - v2.Size.X + 1;
            int resultY = v1.Size.Y - v2.Size.Y + 1;
            var resultBuffer = _accelerator.Allocate1D<double>(resultX * resultY);
            _kernel_ConvolveValid(resultX * resultY, v1Buffer.View, v2Buffer.View, v1.Size.X, v1.Size.Y, v1.Size.Z, v2.Size.X, v2.Size.Y, v2.Size.Z, resultBuffer.View);
            var gradient = new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(resultX, resultY, 1));
            return gradient;
        }

        public static Volume FullyConnected_Backward(Volume volume, Volume weights, Volume bias, Volume error, double learningRate, out Volume updatedBias, out Volume updatedWeights)
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
            _kernel_FullyConnected_Backward_InputGradient(volume.Size.TotalSize, weightBuffer.View, errorBuffer.View, neurons, volume.Size.X, volume.Size.Y, resultBuffer.View);
            _kernel_FullyConnected_Backward(neurons, volumeBuffer.View, weightBuffer.View, biasBuffer.View, errorBuffer.View, learningRate);
            updatedBias = new Volume(biasBuffer.GetAsArray1D(), bias.Size);
            updatedWeights = new Volume(weightBuffer.GetAsArray1D(), weights.Size);
            bias.SetData(updatedBias.Data);
            weights.SetData(updatedWeights.Data);
            return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
        }

        public static Volume LayeredNormalization_Backward(Volume volume, Volume error)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length))
                {
                    errorBuffer.CopyFromCPU(error.Data);
                    using (var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
                    {
                        _kernel_LayeredNormalization_Backward(volume.Size.Z, volumeBuffer.View, volume.Size.X, volume.Size.Y, errorBuffer.View, resultBuffer.View);
                        return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
                    }
                }
            }
        }
    }
}
