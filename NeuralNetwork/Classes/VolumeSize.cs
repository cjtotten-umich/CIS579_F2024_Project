using System;
using System.Collections;
using System.Collections.Generic;

namespace NeuralNetwork
{
    public struct VolumeSize : IEquatable<VolumeSize>
    {
        public int X ;
        public int Y; 
        public int Z;

        public int TotalSize
        {
            get 
            {
                return X * Y * Z; 
            }
        }

        public VolumeSize(int x, int y, int z)
        {
            if (x == 0 || y == 0 )
            {
                throw new ArgumentException("You can't have a volume with 0 length in X or Y dimensions");
            }

            X = x;
            Y = y;
            Z = z;
        }   

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", X, Y, Z);
        }

        public new bool Equals(object x, object y)
        {
            throw new System.NotImplementedException();
        }

        public bool Equals(VolumeSize other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
    }
}
