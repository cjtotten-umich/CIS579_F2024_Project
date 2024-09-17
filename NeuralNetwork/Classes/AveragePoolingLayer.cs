using System;

namespace NeuralNetwork
{
    public class AveragePoolingLayer : Layer
    {
        public AveragePoolingLayer(VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = new VolumeSize(inputVolumeSize.X / 2, inputVolumeSize.Y / 2, inputVolumeSize.Z);
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.AveragePool(volume);
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            if (volume.Size.X != InputVolumeSize.X / 2 && volume.Size.Y != InputVolumeSize.Y / 2 && volume.Size.Z != InputVolumeSize.Z)
            {
                throw new ArgumentException("Invalid error size to back propegate");
            }

            return Processing.AveragePool_Backward(volume, error);
        }

        public override string ToString()
        {
            return "AVGPOOL " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
