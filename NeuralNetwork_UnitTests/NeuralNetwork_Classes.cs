﻿namespace NeuralNetwork_UnitTests
{
    using NeuralNetwork;
    using NUnit.Framework;

    public class NeuralNetwork_Classes
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Volume_AddVolumeToVolume()
        {
            var data1 = Volume.MakeRandom(new VolumeSize(2, 2, 2));
            var data2 = Volume.MakeRandom(new VolumeSize(2, 2, 2));
            var sum = data1 + data2;
            for (int i = 0; i < data1.Size.TotalSize; i++) 
            {
                Assert.That(sum.Data[i] == data1.Data[i] + data2.Data[i]);
            }
        }

        [Test]
        public void Volume_AddVolumeToFloat()
        {
            var volume = Volume.MakeZero(new VolumeSize(2, 2, 2));
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(0 == volume.Data[i]);
            }

            volume += 5;
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(5 ==  volume.Data[i]);
            }
        }

        [Test]
        public void Volume_MultiplyVolumeByFloat()
        {
            var volume = Volume.MakeRandom(new VolumeSize(2, 2, 2));
            var product = volume * 5;
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(product.Data[i] == volume.Data[i] * 5);
            }
        }

        [Test]
        public void MaxPoolingLayer_Forward()
        {
            var layer = new MaxPoolingLayer(new VolumeSize(4, 4, 1), 2);
            var volume = layer.Process(new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1)));

            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(6, volume.Data[0]));
            Assert.That(TestData.AboutEqual(8, volume.Data[1]));
            Assert.That(TestData.AboutEqual(14, volume.Data[2]));
            Assert.That(TestData.AboutEqual(16, volume.Data[3]));

            layer = new MaxPoolingLayer(new VolumeSize(4, 4, 2), 2);
            volume = layer.Process(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)));
            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);
            Assert.That(2 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(6, volume.Data[0]));
            Assert.That(TestData.AboutEqual(8, volume.Data[1]));
            Assert.That(TestData.AboutEqual(14, volume.Data[2]));
            Assert.That(TestData.AboutEqual(16, volume.Data[3]));

            Assert.That(TestData.AboutEqual(22, volume.Data[4]));
            Assert.That(TestData.AboutEqual(24, volume.Data[5]));
            Assert.That(TestData.AboutEqual(30, volume.Data[6]));
            Assert.That(TestData.AboutEqual(32, volume.Data[7]));
        }

        [Test]
        public void MaxPoolingLayer_BackPropegate()
        {
            var layer = new MaxPoolingLayer(new VolumeSize(4, 4, 2), 2);
            var error = new Volume(new double[] { 1, 2, 3, 4, 4, 3, 2, 1 }, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error);
            Assert.That(1 == volume.Data[5]);
            Assert.That(2 == volume.Data[7]);
            Assert.That(3 == volume.Data[13]);
            Assert.That(4 == volume.Data[15]);
            Assert.That(4 == volume.Data[21]);
            Assert.That(3 == volume.Data[23]);
            Assert.That(2 == volume.Data[29]);
            Assert.That(1 == volume.Data[31]);

            layer = new MaxPoolingLayer(new VolumeSize(5, 5, 2), 5);
            error = new Volume(TestData.TestData_1_1_2, new VolumeSize(1, 1, 2));
            volume = layer.BackPropegate(new Volume(TestData.TestData_5_5_2, new VolumeSize(5, 5, 2)), error);
            Assert.That(1 == volume.Data[24]);
            Assert.That(2 == volume.Data[49]);
        }

        [Test]
        public void AveragePoolingLayer_BackPropegate()
        {
            var layer = new AveragePoolingLayer(new VolumeSize(4, 4, 2), 2);
            var error = new Volume(new double[] { 1, 2, 3, 4, 4, 3, 2, 1 }, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error);
            Assert.That((1 / 14.0) * 1 == volume.Data[0]);
            Assert.That((3 / 22.0) * 2 == volume.Data[2]);
            Assert.That((9 / 46.0) * 3 == volume.Data[8]);
            Assert.That((11 / 54.0) * 4 == volume.Data[10]);
            Assert.That((17 / 78.0) * 4 == volume.Data[16]);
            Assert.That((19 / 86.0) * 3 == volume.Data[18]);
            Assert.That((25 / 110.0) * 2 == volume.Data[24]);
            Assert.That((27 / 118.0) * 1 == volume.Data[26]);

            layer = new AveragePoolingLayer(new VolumeSize(5, 5, 2), 5);
            error = new Volume(TestData.TestData_1_1_2, new VolumeSize(1, 1, 2));
            volume = layer.BackPropegate(new Volume(TestData.TestData_5_5_2, new VolumeSize(5, 5, 2)), error);
            for (int i = 0; i < 25; i++)
            {
                Assert.That(((i + 1) / 325.0) == volume.Data[i]);
                Assert.That(2 * ((i + 26) / 950.0) == volume.Data[i + 25]);
            }
        }

        [Test]
        public void ReluLayer_BackPropegate()
        {
            var layer = new ReluActivationLayer(new VolumeSize(4, 4, 2));
            var error = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(TestData.AboutEqual(i + 1, volume.Data[i]));
            }

            volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)) * -1, error);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(TestData.AboutEqual(0, volume.Data[i]));
            }
        }

        [Test]
        public void SigmoidLayer_BackPropegate()
        {
            var layer = new SigmoidActivationLayer(new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error);
            Assert.That(TestData.AboutEqual(0.196611933241, volume.Data[0]));
            Assert.That(TestData.AboutEqual(0.104993585403, volume.Data[1]));
            Assert.That(TestData.AboutEqual(0.045176659730, volume.Data[2]));
            Assert.That(TestData.AboutEqual(0.017662706213, volume.Data[3]));
            Assert.That(TestData.AboutEqual(0.006648056670, volume.Data[4]));
            Assert.That(TestData.AboutEqual(0.002466509291, volume.Data[5]));
            Assert.That(TestData.AboutEqual(0.000910221180, volume.Data[6]));
            Assert.That(TestData.AboutEqual(0.000335237670, volume.Data[7]));

            volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)) * -1, error);
            Assert.That(TestData.AboutEqual(0.196611933241, volume.Data[0]));
            Assert.That(TestData.AboutEqual(0.104993585403, volume.Data[1]));
            Assert.That(TestData.AboutEqual(0.045176659730, volume.Data[2]));
            Assert.That(TestData.AboutEqual(0.017662706213, volume.Data[3]));
            Assert.That(TestData.AboutEqual(0.006648056670, volume.Data[4]));
            Assert.That(TestData.AboutEqual(0.002466509291, volume.Data[5]));
            Assert.That(TestData.AboutEqual(0.000910221180, volume.Data[6]));
            Assert.That(TestData.AboutEqual(0.000335237670, volume.Data[7]));
        }

        [Test]
        public void FullyConnectedLayer_BackPropegate()
        {
            var layer = new FullyConnectedLayer(1, new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_1_1_1, new VolumeSize(1, 1, 1));
            for (int i = 0; i < layer.Weights.Data.Length; i++)
            {
                layer.Weights.Data[i] = 1;
            }

            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error);
            Assert.That(layer.Bias.Data[0] == -1);
            Assert.That(volume.Size.Equals(new VolumeSize(2, 2, 2)));
            for(int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(volume.Data[i] == 1);
            }

            layer = new FullyConnectedLayer(5, new VolumeSize(4, 4, 3));
            error = new Volume(new double[] { 1, 1, 1, 1, 1 } , new VolumeSize(5, 1, 1));
            for (int i = 0; i < layer.Weights.Data.Length; i++)
            {
                layer.Weights.Data[i] = 1;
            }

            volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_3, new VolumeSize(4, 4, 3)), error);
            Assert.That(layer.Bias.Data[0] == -1);
            Assert.That(volume.Size.Equals(new VolumeSize(4, 4, 3)));
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(volume.Data[i] == 1);
            }
        }

        [Test]
        public void ConvolutionLayer_BackPropegate()
        {
            var layer = new ConvolutionLayer(2, 3, new VolumeSize(4, 4, 2));
            var error = new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error);
            Assert.That(1 < 0);
        }
    }
}
