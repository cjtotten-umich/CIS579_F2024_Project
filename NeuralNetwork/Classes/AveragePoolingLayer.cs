using ILGPU.IR;
using ILGPU.Runtime.OpenCL;
using System;

namespace NeuralNetwork
{
    public class AveragePoolingLayer : Layer
    {
        public int PoolSize { get; set; }

        public AveragePoolingLayer(VolumeSize inputVolumeSize, int poolSize)
        {
            int remainder;
            Math.DivRem(inputVolumeSize.X, poolSize, out remainder);
            if (remainder != 0)
            {
                throw new ArgumentException("Volume X size not divisible by pool size");
            }

            Math.DivRem(inputVolumeSize.Y, poolSize, out remainder);
            if (remainder != 0)
            {
                throw new ArgumentException("Volume Y size not divisible by pool size");
            }

            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = new VolumeSize(inputVolumeSize.X / poolSize, inputVolumeSize.Y / poolSize, inputVolumeSize.Z);
            PoolSize = poolSize;
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.AveragePool(volume, PoolSize);
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            if (!error.Size.Equals(OutputVolumeSize))
            {
                throw new ArgumentException("Invalid error size to back propegate");
            }

            return Processing.AveragePool_Backward(volume, error, PoolSize);
        }

        public override string ToString()
        {
            return "AVGPOOL " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
