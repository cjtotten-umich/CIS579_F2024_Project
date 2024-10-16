using ILGPU.Runtime.Cuda;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class DropoutLayer : Layer
    {
        public double DropoutRate { get; set; }

        public bool IsTraining { get; set; }

        private Random _random = new Random();

        public DropoutLayer(double dropoutRate)
        {
            DropoutRate = dropoutRate;
        }

        private double[] _mask;

        public override Volume Process(Volume volume)
        {
            _mask = new double[volume.Size.TotalSize];

            if (IsTraining)
            {
                for (int i = 0; i < _mask.Length; i++)
                {
                    _mask[i] = _random.NextDouble() < DropoutRate ? 0 : 1;
                }
            }
            else
            {
                for (int i = 0; i < _mask.Length; i++)
                {
                    _mask[i] = 1 - DropoutRate; // Scale neurons in inference
                }
            }

            // Apply the mask to the input values
            //double[] outputs = new double[size];
            //for (int i = 0; i < size; i++)
            //{
            //    outputs[i] = inputs[i] * mask[i];
            //}

            return null;
        }

        public override Volume BackPropegate(Volume volume, Volume error, bool verbose)
        {
            throw new System.NotImplementedException();
        }
    }
}
