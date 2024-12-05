using System;

namespace NeuralNetwork
{
    public abstract class Layer
    {
        public abstract Volume Process(Volume volume);

        public abstract Volume BackPropegate(Volume volume, Volume error, bool verbose);

        public Volume Train(Volume volume, Volume truth, bool verbose)
        {
            var result = Process(volume);

            if (verbose)
            {
                Console.WriteLine(this + " - TRAINING");
            }

            if (NextLayer == null)
            {
                return BackPropegate(volume, Processing.StopSignError(result, truth), verbose);
            }

            var error = NextLayer.Train(result, truth, verbose);
            return BackPropegate(volume, error, verbose);
        }

        public Layer NextLayer { get; set; }

        public Layer PreviousLayer { get; set; }

        public VolumeSize OutputVolumeSize { get; set; }

        public VolumeSize InputVolumeSize { get; set; }
    }
}
