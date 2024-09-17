namespace NeuralNetwork
{
    using System;

    public class FullyConnectedLayer : Layer
    {
        public Volume Weights { get; set; }

        public Volume Bias { get; set; }

        public FullyConnectedLayer(int neurons, VolumeSize inputVolumeSize)
        { 
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = new VolumeSize(neurons, 1, 1);
            Weights = Volume.MakeRandom(new VolumeSize(InputVolumeSize.TotalSize, neurons, 1));
            Bias = Volume.MakeZero(new VolumeSize(neurons, 0, 0));
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.FullyConnected(volume, Weights, Bias);
        }

        public override Volume BackPropegate(Volume volume, Volume error)
        {
            Volume updatedBias;
            Volume updatedWeights;
            return Processing.FullyConnected_Backward(volume, Weights, Bias, error, out updatedBias, out updatedWeights);
        }

        public override string ToString()
        {
            return "FC " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
