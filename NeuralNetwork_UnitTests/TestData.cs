namespace NeuralNetwork_UnitTests
{
    using System;

    public class TestData
    {
        public static double Epsilon = 0.00000001f;

        public static double[] TestData_4_4_3 = new double[]
        {
            1, 2,  3,  4,
            5, 6,  7,  8,
            9, 10, 11, 12,
            13,14, 15, 16,

            17, 18, 19, 20,
            21, 22, 23, 24,
            25, 26, 27, 28,
            29, 30, 31, 32,

            33, 34, 35, 36,
            37, 38, 39, 40,
            41, 42, 43, 44,
            45, 46, 47, 48
        };
        
        public static double[] TestData_5_5_2 = new double[]
        {
            1, 2,  3,  4, 5,
            6, 7, 8, 9, 10,
            11, 12, 13, 14, 15,
            16, 17, 18, 19, 20,
            21, 22, 23, 24, 25,

            26, 27, 28, 29, 30,
            31, 32, 33, 34, 35,
            36, 37, 38, 39, 40, 
            41, 42, 43, 44, 45,
            46, 47, 48, 49, 50
        };

        public static double[] TestData_4_4_1 = new double[]
        {
            1, 2,  3,  4,
            5, 6,  7,  8,
            9, 10, 11, 12,
            13,14, 15, 16
        };

        public static double[] TestData_4_4_2 = new double[]
        {
            1, 2,  3,  4,
            5, 6,  7,  8,
            9, 10, 11, 12,
            13,14, 15, 16,

            17, 18, 19, 20,
            21, 22, 23, 24,
            25, 26, 27, 28,
            29, 30, 31, 32
        };

        public static double[] TestData_1_1_2 = new double[]
        {
            1, 
            
            2
        };

        public static double[] TestData_3_3_3 = new double[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9,

            10, 11, 12,
            13, 14, 15,
            16, 17, 18,

            19, 20, 21,
            22, 23, 24,
            25, 26, 27
        };

        public static double[] TestData_2_2_3 = new double[]
        {
            1, 2,
            3, 4,

            5, 6,
            7, 8,

            9, 10,
            11, 12
        };

        public static double[] TestData_3_3_1 = new double[]
        {
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        };

        public static double[] TestData_2_2_2 = new double[]
        {
            1, 2,
            3, 4,

            5, 6,
            7, 8
        };

        public static double[] TestData_1_1_1 = new double[]
        {
            1
        };

        public static double[] TestData_2_2_1 = new double[]
        {
            1, 2,
            3, 4
        };

        public static double[] TestWeights_2_2_2_2 = new double[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,

            9, 10, 11, 12,
            13, 14, 15, 16
        };

        public static bool AboutEqual(double v1, double v2)
        {
            return Math.Abs(v1 - v2) <= Epsilon;
        }
    }
}
