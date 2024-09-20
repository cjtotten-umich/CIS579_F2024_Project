namespace NeuralNetwork
{
    using System;

    public class MaxPoolingLayer : Layer
    {
        public int PoolSize { get; set; }

        public MaxPoolingLayer(VolumeSize inputVolumeSize, int poolSize)
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

            return Processing.MaxPool(volume, PoolSize);
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

            return Processing.MaxPool_Backward(volume, error, PoolSize);
        }

        public override string ToString()
        {
            return "MAXPOOL " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
