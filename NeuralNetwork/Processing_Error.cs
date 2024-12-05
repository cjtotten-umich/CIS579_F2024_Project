namespace NeuralNetwork
{
    using ILGPU.IR;
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

        public static Volume ComponentError(Volume result, Volume expected)
        {
            if (!result.Size.Equals(expected.Size))
            {
                throw new ArgumentException("Mismatched size of result vs expected");
            }

            return result - expected;
        }

        public static Volume StopSignError(Volume result, Volume expected)
        {
            if (!result.Size.Equals(expected.Size))
            {
                throw new ArgumentException("Mismatched size of result vs expected");
            }

            var value = result - expected;

            // If the expected is 0, the error on distance should be ignored
            if (expected.Data[0] == 0)
            {
                value.Data[1] = 0;
                value.Data[2] = 0;
            }

            return value;
        }
    }
}
