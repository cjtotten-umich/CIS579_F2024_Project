namespace NeuralNetwork
{
    using ILGPU;
    using ILGPU.Runtime.Cuda;
    using ILGPU.Runtime;
    using ILGPU.Runtime.CPU;

    public partial class Processing
    {
        static Context _context;
        static Accelerator _accelerator;

        static Processing()
        {
            _context = Context.Create(b => b.Default().EnableAlgorithms());
            //_accelerator = _context.CreateCudaAccelerator(0);
            _accelerator = _context.CreateCPUAccelerator(0);

            _kernel_24BPP_RGB_ImageToVolume = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, int, int, ArrayView<double>>(Kernel_24BPP_RGB_ImageToVolume);
            _kernel_32BPP_ARGB_ImageToVolume = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, int, int, ArrayView<double>>(Kernel_32BPP_ARGB_ImageToVolume);
            
            _kernel_MaxPool = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>>(Kernel_MaxPool);
            _kernel_MaxPool_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>>(Kernel_MaxPool_Backward);
            
            _kernel_ConvolveVolumeWithFilter = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, double, ArrayView<double>>(Kernel_ConvolveVolumeWithFilter);
            _kernel_ConvolveVolumeWithFilter_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>, int, int, ArrayView<double>>(Kernel_ConvolveVolumeWithFilter_Backward);
            
            _kernel_FullyConnected = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>>(Kernel_FullyConnected);
            _kernel_FullyConnected_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>, ArrayView<double>>(Kernel_FullyConnected_Backward);

            _kernel_Sigmoid = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>>(Kernel_Sigmoid);
            _kernel_Sigmoid_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>>(Kernel_Sigmoid_Backward);
            
            _kernel_Relu = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>>(Kernel_Relu);
            _kernel_Relu_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>, ArrayView<double>>(Kernel_Relu_Backward);

            _kernel_AveragePool = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>>(Kernel_AveragePool);
            _kernel_AveragePool_Backward = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, int, int, int, ArrayView<double>, ArrayView<double>>(Kernel_AveragePool_Backward);
        }
    }
}
