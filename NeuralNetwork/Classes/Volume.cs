using System;
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
                data[i] = random.NextDouble();
            }

            return new Volume(data, size);
        }

        public static Volume MakeValue(VolumeSize size, double value)
        {
            var data = new double[size.TotalSize];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = value;
            }

            return new Volume(data, size);
        }

        public static Volume MakeHeUniform(VolumeSize size)
        {
            var heSize = Math.Sqrt(6.0 / size.TotalSize);
            var random = new Random();
            var data = new double[size.TotalSize];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (random.NextDouble() * heSize * 2) - heSize;
            }

            return new Volume(data, size);
        }

        public static Volume MakeZero(VolumeSize size)
        {
            var random = new Random();
            var data = new double[size.TotalSize];
            return new Volume(data, size);
        }

        public static Volume MakeEmpty(VolumeSize size)
        {
            if (size.Z != 0)
            {
                throw new ArgumentException("Empty volume must have Z = 0");
            }

            return new Volume(new double[0], size);
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

        public static Volume operator /(Volume a, double value)
        {
            var data = new double[a.Size.TotalSize];

            for (int i = 0; i < a.Size.TotalSize; i++)
            {
                data[i] = a.Data[i] / value;
            }

            return new Volume(data, a.Size);
        }

        public Volume Slice(int depth)
        {
            var layerSize = Size.X * Size.Y;
            var offset = layerSize * depth;
            var data = new double[layerSize];
            Buffer.BlockCopy(Data, offset * sizeof(double), data, 0, layerSize * sizeof(double));
            return new Volume(data, new VolumeSize(Size.X, Size.Y, 1));
        }

        public Volume Append(Volume other)
        {
            if (Size.X != other.Size.X ||  Size.Y != other.Size.Y)
            {
                throw new ArgumentException("Size mismatch, can't append different size volumes");
            }
            var data = new double[Size.TotalSize + other.Size.TotalSize];
            Buffer.BlockCopy(Data, 0, data, 0, Size.TotalSize * sizeof(double));
            Buffer.BlockCopy(other.Data, 0, data, Size.TotalSize * sizeof(double), other.Size.TotalSize * sizeof(double));
            return new Volume(data, new VolumeSize(Size.X, Size.Y, Size.Z + other.Size.Z));
        }

        public Volume Flip()
        {
            return Processing.VolumeFlip(this);
        }

        public Volume Pad(int padSize)
        {
            return Processing.VolumePad(this, padSize);
        }

        public string StringVersion()
        {
            if (Data.Length < 10)
            {
                var s = new StringBuilder();
                s.Append("(");
                for (int i = 0; i < Data.Length; i++)
                {
                    s.Append(Data[i].ToString("N4") + ",");
                }

                s.Append(")");
                return s.ToString();
            }

            return "Volume:" + Size.ToString();

        }
    }
}
