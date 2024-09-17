namespace NeuralNetwork
{
    using System;

    public partial class Processing
    {
        public static double MeanSquareError(Volume volume, Volume expected)
        {
            if (!volume.Size.Equals(expected.Size))
            {
                throw new ArgumentException("Mismatched size of values vs expected");
            }

            var sum = 0.0;
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                sum += Math.Pow(volume.Data[i] - expected.Data[i], 2);
            }

            return sum / volume.Size.TotalSize;
        }

        public static Volume ComponentError(Volume volume, Volume expected)
        {
            if (!volume.Size.Equals(expected.Size))
            {
                throw new ArgumentException("Mismatched size of values vs expected");
            }

            return expected - volume;
        }
    }
}
