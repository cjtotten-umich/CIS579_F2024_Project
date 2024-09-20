namespace NeuralNetwork
{
    using ILGPU.Algorithms;
    using ILGPU;
    using ILGPU.Runtime;
    using System;

    public partial class Processing
    {
        static Action<Index1D, ArrayView<double>, ArrayView<double>> _kernel_Sigmoid;
        static Action<Index1D, ArrayView<double>, ArrayView<double>> _kernel_Relu;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_Sigmoid_Backward;
        static Action<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>> _kernel_Relu_Backward;

        static void Kernel_Sigmoid(Index1D index, ArrayView<double> volume, ArrayView<double> result)
        {
            result[index] = 1.0 / (1.0 + Math.Exp(volume[index] * -1));
        }

        static void Kernel_Sigmoid_Backward(Index1D index, ArrayView<double> volume, ArrayView<double> error, ArrayView<double> result)
        {
            var sigmoid = 1.0 / (1.0 + Math.Exp(volume[index] * -1));
            result[index] = error[index] * (sigmoid * (1 - sigmoid));
        }

        static void Kernel_Relu(Index1D index, ArrayView<double> volume, ArrayView<double> result)
        {
            if (volume[index] >= 0)
            {
                result[index] = volume[index];
            }
            else
            {
                result[index] = 0;
            }
        }

        static void Kernel_Relu_Backward(Index1D index, ArrayView<double> volume, ArrayView<double> error, ArrayView<double> result)
        {
            if (volume[index] >= 0)
            {
                result[index] = error[index];
            }
            else
            {
                result[index] = 0;
            }
        }

        public static Volume Relu(Volume volume)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
                {
                    _kernel_Relu(volume.Data.Length, volumeBuffer.View, resultBuffer.View);
                    var result = resultBuffer.GetAsArray1D();
                    return new Volume(result, volume.Size);
                }
            }
        }

        public static Volume Sigmoid(Volume volume)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
                { 
                    _kernel_Sigmoid(volume.Data.Length, volumeBuffer.View, resultBuffer.View);
                    var result = resultBuffer.GetAsArray1D();
                    return new Volume(result, volume.Size);
                }
            }
        }

        public static Volume Relu_Backward(Volume volume, Volume error)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length))
                {
                    errorBuffer.CopyFromCPU(error.Data);
                    var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
                    _kernel_Relu_Backward(volume.Data.Length, volumeBuffer.View, errorBuffer.View, resultBuffer.View);
                    return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
                }
            }
        }

        public static Volume Sigmoid_Backward(Volume volume, Volume error)
        {
            using (var volumeBuffer = _accelerator.Allocate1D<double>(volume.Data.Length))
            {
                volumeBuffer.CopyFromCPU(volume.Data);
                using (var errorBuffer = _accelerator.Allocate1D<double>(error.Data.Length))
                {
                    errorBuffer.CopyFromCPU(error.Data);
                    var resultBuffer = _accelerator.Allocate1D<double>(volume.Data.Length);
                    _kernel_Sigmoid_Backward(volume.Data.Length, volumeBuffer.View, errorBuffer.View, resultBuffer.View);
                    return new Volume(resultBuffer.GetAsArray1D(), volume.Size);
                }
            }
        }
    }
}
