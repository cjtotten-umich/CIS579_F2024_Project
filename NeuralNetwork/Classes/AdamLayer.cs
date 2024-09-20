using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class AdamLayer : Layer
    {
        public double[] Momentums { get; set; }

        public double[] Variances { get; set; }

        public double MomentumDecay = 0.9;

        public double VarianceDecay = 0.999;

        public int TrainingStep = 1;

        public AdamLayer(VolumeSize inputVolumeSize)
        {
            InputVolumeSize = inputVolumeSize;
            OutputVolumeSize = inputVolumeSize;
            Momentums = new double[inputVolumeSize.TotalSize];
            Variances = new double[inputVolumeSize.TotalSize];
        }

        public override Volume BackPropegate(Volume volume, Volume error, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine(this + " - BACKPROP");
            }

            var results = new double[error.Size.TotalSize];
            for (int i = 0; i < Momentums.Length; i++)
            {
                var result = Processing.Adam(Momentums[i], Variances[i], TrainingStep, error.Data[i]);
                Momentums[i] = result.Item1;
                Variances[i] = result.Item2;
                results[i] = result.Item3;
             }

            TrainingStep++;

            return new Volume(results, volume.Size);
        }

        public override Volume Process(Volume volume)
        {
            return volume;
        }

        public override string ToString()
        {
            return "ADAM " + InputVolumeSize + "-" + OutputVolumeSize;
        }
    }
}
