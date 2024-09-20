namespace NeuralNetwork
{
    using System;

    public class FullyConnectedLayer : Layer
    {
        public Volume Weights { get; set; }

        public Volume Bias { get; set; }

        public double LearningRate { get; set; }

        public FullyConnectedLayer(int neurons, double learningRate, VolumeSize inputVolumeSize)
        { 
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = new VolumeSize(neurons, 1, 1);
            Weights = Volume.MakeRandom(new VolumeSize(InputVolumeSize.TotalSize, neurons, 1));
            Bias = Volume.MakeZero(new VolumeSize(neurons, 1, 1));
            LearningRate = learningRate;
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.FullyConnected(volume, Weights, Bias);
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

            Volume updatedBias;
            Volume updatedWeights;
            var result = Processing.FullyConnected_Backward(volume, Weights, Bias, error, LearningRate, out updatedBias, out updatedWeights);
            Weights = updatedWeights;
            Bias = updatedBias;
            return result;
        }

        public override string ToString()
        {
            return "FC " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
