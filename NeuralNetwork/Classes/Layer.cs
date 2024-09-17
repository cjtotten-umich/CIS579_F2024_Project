using System;

namespace NeuralNetwork
{
    public abstract class Layer
    {
        public abstract Volume Process(Volume volume);

        public abstract Volume BackPropegate(Volume volume, Volume error);

        public Volume Train(Volume volume, Volume truth)
        {
            var result = Process(volume);

            Console.WriteLine(this + " - TRAINING");
            if (NextLayer == null)
            {
                return BackPropegate(volume, Processing.ComponentError(result, truth));
            }

            var error = NextLayer.Train(result, truth);
            return BackPropegate(volume, error);
        }

        public Layer NextLayer { get; set; }

        public Layer PreviousLayer { get; set; }

        public VolumeSize OutputVolumeSize { get; set; }

        public VolumeSize InputVolumeSize { get; set; }
    }
}
