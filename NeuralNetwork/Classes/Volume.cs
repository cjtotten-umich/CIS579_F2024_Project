using ILGPU.Algorithms.MatrixOperations;
using ILGPU.IR;
using ILGPU.IR.Values;
using System;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text;

namespace NeuralNetwork
{
    public class Volume
    {
        public double[] Data { get; set; }

        public VolumeSize Size { get; set; }

        public Volume(double[] data, VolumeSize size)
        {
            if (data.Length != size.TotalSize)
            {
                throw new ArgumentException("Size Issue on volume");
            }

            Data = data; 
            Size = size;
        }
        
        public void SetData(double[] newData)
        {
            Data = newData;
        }

        public static Volume MakeRandom(VolumeSize size)
        {
            var random = new Random();
            var data = new double[size.TotalSize];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (double)random.NextDouble();
            }

            return new Volume(data, size);
        }

        public static Volume MakeZero(VolumeSize size)
        {
            var random = new Random();
            var data = new double[size.TotalSize];
            return new Volume(data, size);
        }

        public static Volume operator +(Volume a, Volume b)
        {
            var data = new double[a.Size.TotalSize];

            for (int i = 0; i < a.Size.TotalSize; i++)
            {
                data[i] = a.Data[i] + b.Data[i];
            }

            return new Volume(data, a.Size);
        }

        public static Volume operator +(Volume a, double value)
        {
            var data = new double[a.Size.TotalSize];

            for (int i = 0; i < a.Size.TotalSize; i++)
            {
                data[i] = a.Data[i] + value;
            }

            return new Volume(data, a.Size);
        }

        public static Volume operator -(Volume a, Volume b)
        {
            return a + (b * -1);
        }

        public static Volume operator -(Volume a, double value)
        {
            return a + (value * -1);
        }

        public static Volume operator *(Volume a, double value)
        {
            var data = new double[a.Size.TotalSize];

            for (int i = 0; i < a.Size.TotalSize; i++)
            {
                data[i] = a.Data[i] * value;
            }

            return new Volume(data, a.Size);
        }
    }
}
