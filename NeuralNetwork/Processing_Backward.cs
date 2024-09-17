namespace NeuralNetwork
{
    using ILGPU;
    using ILGPU.IR.Transformations;
    using ILGPU.Runtime;
    using System;

    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, double, ArrayView<double>, ArrayView<double>> _kernel_ConvolveVolumeWithFilter_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>> _kernel_MaxPool_Backward;
        static Action<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>> _kernel_AveragePool_Backward;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_FullyConnected_Backward;

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
            var sum = 0.0;
            var weightOffset = volume.Length * index;
            for (int i = 0; i < volume.Length; i++)
            {
                weights[weightOffset + i] = 0.0;
                result[weightOffset + i] = error[index] * 
            }

            bias[index] = bias[index] - error[index];
            result[index] = sum + bias;
        }

        static void Kernel_ConvolveVolumeWithFilter_Backward(Index1D index, ArrayView<double> volume, int x, int y, int z, ArrayView<double> filter, int filterX, int filterY, int filterZ, double bias, ArrayView<double> error, ArrayView<double> result)
        {
            //var offsetX = index % x;
            //var offsetY = index / x;
            //var halfFilterX = filterX / 2;
            //var halfFilterY = filterY / 2;

            //var offset = index - (halfFilterX) - (halfFilterY * x);
            //var filterSize = filter.Length;

            //double sum = 0;
            //int currentX = 0;
            //int currentY = 0;
            //int currentZ = 0;
            //for (int i = 0; i < filterSize; i++)
            //{
            //    var currentIndex = offset + currentX + (x * currentY) + (x * y * currentZ);
            //    if (currentX + offsetX >= halfFilterX &&
            //        currentY + offsetY >= halfFilterY &&
            //        currentX - halfFilterX + offsetX < x &&
            //        currentY - halfFilterY + offsetY < y)
            //    {
            //        sum += volume[currentIndex] * filter[i];
            //    }

            //    currentX++;
            //    if (currentX >= filterX)
            //    {
            //        currentX = 0;
            //        currentY++;
            //        if (currentY >= filterY)
            //        {
            //            currentY = 0;
            //            currentZ++;
            //        }
            //    }
            //}

            //result[index] = sum + bias;
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

        public static Volume Convolve_Backward(Volume volume, Volume error)
        {
            var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
            var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length);
            volumeBuffer.CopyFromCPU(volume.Data);
            errorBuffer.CopyFromCPU(error.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(volume.Size.TotalSize);
            //_kernel_ConvolveVolumeWithFilter_Backward(volume.Size.X * volume.Size.Y, volumeBuffer.View, volume.Size.X, volume.Size.Y, volume.Size.Z, filterBuffer.View, filter.Size.X, filter.Size.Y, filter.Size.Z, bias, errorBuffer.View, resultBuffer.View); 
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
            errorBuffer.CopyFromCPU(bias.Data);
            var resultBuffer = _accelerator.Allocate1D<double>(neurons);
            _kernel_FullyConnected_Backward(neurons, volumeBuffer.View, weightBuffer.View, biasBuffer.View, errorBuffer.View, resultBuffer.View);
            updatedBias = new Volume(biasBuffer.GetAsArray1D(), bias.Size);
            updatedWeights = new Volume(weightBuffer.GetAsArray1D(), weights.Size);
            return new Volume(resultBuffer.GetAsArray1D(), new VolumeSize(neurons, 1, 1));
        }
    }
}
