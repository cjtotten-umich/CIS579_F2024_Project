using System;

namespace NeuralNetwork
{
    public class LayeredNormalizationLayer : Layer
    {
        public LayeredNormalizationLayer(VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = inputVolumeSize;
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

            if (!error.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Error volume is the wrong size");
            }

            return Processing.LayeredNormalization_Backward(volume, error);
        }

        public override Volume Process(Volume volume)
        {
            if (!volume.Size.Equals(InputVolumeSize))
            {
                throw new ArgumentException("Input volume is the wrong size");
            }

            return Processing.LayeredNormalization(volume);
        }

        public override string ToString()
        {
            return "NORM " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
