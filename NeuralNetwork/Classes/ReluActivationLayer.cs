using System;

namespace NeuralNetwork
{
    public class ReluActivationLayer : Layer
    {
        public ReluActivationLayer(VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = inputVolumeSize;
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            if (!volume.Size.Equals(error.Size))
            {
                throw new ArgumentException("Invalid error size to back propegate");
            }

            return Processing.Relu_Backward(volume, error);
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.Relu(volume);
        }

        public override string ToString()
        {
            return "RELU " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
