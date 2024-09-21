namespace NeuralNetwork_UnitTests
{
    using NeuralNetwork;
    using NUnit.Framework;
    using System;

    public class NeuralNetwork_Classes
    {
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

            Assert.That(true);
        }

        [Test]
        public void Volume_AddFloatToValue()
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

            Assert.That(true);
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

            Assert.That(true);
        }

        [Test]
        public void Volume_SliceVolume()
        {
            var volume = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            var firstLayer = volume.Slice(0);
            Assert.That(4 == firstLayer.Size.X);
            Assert.That(4 == firstLayer.Size.Y);
            Assert.That(1 == firstLayer.Size.Z);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(i + 1 == firstLayer.Data[i]);
            }

            var secondLayer = volume.Slice(1);
            Assert.That(4 == secondLayer.Size.X);
            Assert.That(4 == secondLayer.Size.Y);
            Assert.That(1 == secondLayer.Size.Z);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(i + 17 == secondLayer.Data[i]);
            }

            Assert.That(true);
        }

        [Test]
        public void Volume_Flip()
        {
            var volume = new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1));
            var flipped = volume.Flip();
            for (int i = 0; i < 16; i++)
            {
                Assert.That(16 - i == flipped.Data[i]);
            }

            volume = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            flipped = volume.Flip();
            for (int i = 0; i < 16; i++)
            {
                Assert.That(16 - i == flipped.Data[i]);
                Assert.That(32 - i == flipped.Data[i + 16]);
            }

            Assert.That(true);
        }

        [Test]
        public void Volume_Pad()
        {
            var pad = 2;
            var volume = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            var padded = volume.Pad(pad);
            var i = 0;
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(1, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(2, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(3, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(4, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(5, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(6, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(7, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(8, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(9, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(10, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(11, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(12, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(13, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(14, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(15, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(16, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));

            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(17, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(18, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(19, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(20, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(21, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(22, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(23, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(24, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(25, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(26, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(27, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(28, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(29, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(30, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(31, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(32, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));
            Assert.That(TestData.AboutEqual(0, padded.Data[i++]));

            Assert.That(true);
        }

        [Test]
        public void Volume_Append()
        {
            var volume1 = new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1));
            var volume2 = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            var newVolume = volume1.Append(volume2);
            Assert.That(4 == newVolume.Size.X);
            Assert.That(4 == newVolume.Size.Y);
            Assert.That(3 == newVolume.Size.Z);
            for (int i = 0; i < 16; i++)
            {
                Assert.That(i + 1 == newVolume.Data[i]);
                Assert.That(i + 1 == newVolume.Data[i + 16]);
                Assert.That(i + 1 + 16 == newVolume.Data[i + 32]);
            }

            Assert.That(true);
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

            Assert.That(true);
        }

        [Test]
        public void MaxPoolingLayer_BackPropegate()
        {
            var layer = new MaxPoolingLayer(new VolumeSize(4, 4, 2), 2);
            var error = new Volume(new double[] { 1, 2, 3, 4, 4, 3, 2, 1 }, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error, false);
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
            volume = layer.BackPropegate(new Volume(TestData.TestData_5_5_2, new VolumeSize(5, 5, 2)), error, false);
            Assert.That(1 == volume.Data[24]);
            Assert.That(2 == volume.Data[49]);

            Assert.That(true);
        }

        [Test]
        public void AveragePoolingLayer_BackPropegate()
        {
            var layer = new AveragePoolingLayer(new VolumeSize(4, 4, 2), 2);
            var error = new Volume(new double[] { 1, 2, 3, 4, 4, 3, 2, 1 }, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error, false);
            Assert.That(0.25 == volume.Data[0]);
            Assert.That(0.25 == volume.Data[1]);
            Assert.That(0.25 == volume.Data[4]);
            Assert.That(0.25 == volume.Data[5]);
            Assert.That(0.5 == volume.Data[2]);
            Assert.That(0.5 == volume.Data[3]);
            Assert.That(0.5 == volume.Data[6]);
            Assert.That(0.5 == volume.Data[7]);
            Assert.That(0.75 == volume.Data[8]);
            Assert.That(0.75 == volume.Data[9]);
            Assert.That(0.75 == volume.Data[12]);
            Assert.That(0.75 == volume.Data[13]);
            Assert.That(1 == volume.Data[10]);
            Assert.That(1 == volume.Data[11]);
            Assert.That(1 == volume.Data[14]);
            Assert.That(1 == volume.Data[15]);

            layer = new AveragePoolingLayer(new VolumeSize(5, 5, 2), 5);
            error = new Volume(TestData.TestData_1_1_2, new VolumeSize(1, 1, 2));
            volume = layer.BackPropegate(new Volume(TestData.TestData_5_5_2, new VolumeSize(5, 5, 2)), error, false);
            for (int i = 0; i < 25; i++)
            {
                Assert.That(0.04 == volume.Data[i]);
                Assert.That(0.08 == volume.Data[i + 25]);
            }

            Assert.That(true);
        }

        [Test]
        public void ReluLayer_BackPropegate()
        {
            var layer = new ReluActivationLayer(new VolumeSize(4, 4, 2));
            var error = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)) * 0.5;
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error, false);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(TestData.AboutEqual((i + 1) / 2.0, volume.Data[i]));
            }

            var v = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)) * -1.0;
            volume = layer.BackPropegate(v, error, false);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(TestData.AboutEqual(0, volume.Data[i]));
            }

            Assert.That(true);
        }

        [Test]
        public void SigmoidLayer_BackPropegate()
        {
            var layer = new SigmoidActivationLayer(new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)) * 2;
            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error, false);
            Assert.That(TestData.AboutEqual(0.39322386648, volume.Data[0]));
            Assert.That(TestData.AboutEqual(0.41997434161, volume.Data[1]));
            Assert.That(TestData.AboutEqual(0.27105995838, volume.Data[2]));
            Assert.That(TestData.AboutEqual(0.14130164970, volume.Data[3]));
            Assert.That(TestData.AboutEqual(0.06648056670, volume.Data[4]));
            Assert.That(TestData.AboutEqual(0.02959811149, volume.Data[5]));
            Assert.That(TestData.AboutEqual(0.01274309652, volume.Data[6]));
            Assert.That(TestData.AboutEqual(0.00536380273, volume.Data[7]));

            error *= -1;
            volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)) * -1, error, false);
            Assert.That(TestData.AboutEqual(-0.39322386648, volume.Data[0]));
            Assert.That(TestData.AboutEqual(-0.41997434161, volume.Data[1]));
            Assert.That(TestData.AboutEqual(-0.27105995838, volume.Data[2]));
            Assert.That(TestData.AboutEqual(-0.14130164970, volume.Data[3]));
            Assert.That(TestData.AboutEqual(-0.06648056670, volume.Data[4]));
            Assert.That(TestData.AboutEqual(-0.02959811149, volume.Data[5]));
            Assert.That(TestData.AboutEqual(-0.01274309652, volume.Data[6]));
            Assert.That(TestData.AboutEqual(-0.00536380273, volume.Data[7]));

            Assert.That(true);
        }

        [Test]
        public void LayerNormalizationLayer_Forward()
        {
            var layer = new LayeredNormalizationLayer(new VolumeSize(2, 2, 2));
            var volume = layer.Process(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)));
            var average = (1 + 2 + 3 + 4) / 4.0;
            var variance = Math.Sqrt((Math.Pow(1 - average, 2) + Math.Pow(2 - average, 2) + Math.Pow(3 - average, 2) + Math.Pow(4 - average, 2)) / 4.0 + 0.0000000001);

            for (int i = 0; i < 4; i++)
            {
                var v = ((i + 1) - average) / variance;
                Assert.That(TestData.AboutEqual(v, volume.Data[i]));
            }

            Assert.That(false);
        }

        [Test]
        public void LayerNormalizationLayer_BackWard()
        {
            var layer = new LayeredNormalizationLayer(new VolumeSize(2, 2, 2));
            var error = new double[8];
            var average = (1 + 2 + 3 + 4) / 4.0;
            var variance = Math.Sqrt((Math.Pow(1 - average, 2) + Math.Pow(2 - average, 2) + Math.Pow(3 - average, 2) + Math.Pow(4 - average, 2)) / 4.0 + 0.0000000001);
            
            for (int i = 0; i < 4; i++)
            {
                error[i] = ((i + 1) - average) / variance;
            }

            average = (5 + 6 + 7 + 8) / 4.0;
            variance = Math.Sqrt((Math.Pow(5 - average, 2) + Math.Pow(6 - average, 2) + Math.Pow(7 - average, 2) + Math.Pow(8 - average, 2)) / 4.0 + 0.0000000001);
            for (int i = 4; i < 8; i++)
            {
                error[i] = ((i + 1) - average) / variance;
            }

            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), new Volume(error, new VolumeSize(2, 2, 2)), false);

            for (int i = 0; i < 8; i++)
            {
                Assert.That(TestData.AboutEqual(i + 1, volume.Data[i]));
            }

            Assert.That(false);
        }

        [Test]
        public void FullyConnectedLayer_BackPropegate()
        {
            var layer = new FullyConnectedLayer(1, 0.01, new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_1_1_1, new VolumeSize(1, 1, 1));
            for (int i = 0; i < layer.Weights.Data.Length; i++)
            {
                layer.Weights.Data[i] = 1;
            }

            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error, false);
            Assert.That(layer.Bias.Data[0] == -0.01);
            Assert.That(volume.Size.Equals(new VolumeSize(2, 2, 2)));
            for(int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(volume.Data[i] == 1);
            }

            Assert.That(TestData.AboutEqual(0.99, layer.Weights.Data[0]));
            Assert.That(TestData.AboutEqual(0.98, layer.Weights.Data[1]));
            Assert.That(TestData.AboutEqual(0.97, layer.Weights.Data[2]));
            Assert.That(TestData.AboutEqual(0.96, layer.Weights.Data[3]));
            Assert.That(TestData.AboutEqual(0.95, layer.Weights.Data[4]));
            Assert.That(TestData.AboutEqual(0.94, layer.Weights.Data[5]));
            Assert.That(TestData.AboutEqual(0.93, layer.Weights.Data[6]));
            Assert.That(TestData.AboutEqual(0.92, layer.Weights.Data[7]));

            layer = new FullyConnectedLayer(5, 0.01, new VolumeSize(4, 4, 3));
            error = new Volume(new double[] { 1, 1, 1, 1, 1 } , new VolumeSize(5, 1, 1));
            for (int i = 0; i < layer.Weights.Data.Length; i++)
            {
                layer.Weights.Data[i] = 0.5;
            }

            for (int i = 0; i < layer.Bias.Data.Length; i++)
            {
                layer.Bias.Data[i] = 0.5;
            }

            volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_3, new VolumeSize(4, 4, 3)), error, false);

            for (int i = 0; i < layer.Bias.Data.Length; i++)
            {
                Assert.That(TestData.AboutEqual(0.49, layer.Bias.Data[i]));
            }

            Assert.That(TestData.AboutEqual(0.49, layer.Weights.Data[0]));
            Assert.That(TestData.AboutEqual(0.48, layer.Weights.Data[1]));
            Assert.That(TestData.AboutEqual(0.47, layer.Weights.Data[2]));
            Assert.That(TestData.AboutEqual(0.46, layer.Weights.Data[3]));
            Assert.That(TestData.AboutEqual(0.45, layer.Weights.Data[4]));
            Assert.That(TestData.AboutEqual(0.44, layer.Weights.Data[5]));
            Assert.That(TestData.AboutEqual(0.43, layer.Weights.Data[6]));
            Assert.That(TestData.AboutEqual(0.42, layer.Weights.Data[7]));


            Assert.That(volume.Size.Equals(new VolumeSize(4, 4, 3)));
            for (int i = 0; i < volume.Size.TotalSize; i++)
            {
                Assert.That(volume.Data[i] == 2.5);
            }

            Assert.That(true);
        }

        [Test]
        public void ConvolutionLayer_BackPropegate()
        {
            var layer = new ConvolutionLayer(2, 3, 0.01, new VolumeSize(4, 4, 2));
            for (int i = 0; i < layer.Filters.Count; i++)
            {
                for (int j = 0; j < layer.Filters[i].Data.Length; j++)
                {
                    layer.Filters[i].Data[j] = 0.5;
                }
            }

            var error = new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error, false);

            int index = 0;
            Assert.That(3 == volume.Data[index++]);
            Assert.That(7 == volume.Data[index++]);
            Assert.That(7 == volume.Data[index++]);
            Assert.That(4 == volume.Data[index++]);
            Assert.That(8 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(10 == volume.Data[index++]);
            Assert.That(8 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(10 == volume.Data[index++]);
            Assert.That(5 == volume.Data[index++]);
            Assert.That(11 == volume.Data[index++]);
            Assert.That(11 == volume.Data[index++]);
            Assert.That(6 == volume.Data[index++]);

            Assert.That(3 == volume.Data[index++]);
            Assert.That(7 == volume.Data[index++]);
            Assert.That(7 == volume.Data[index++]);
            Assert.That(4 == volume.Data[index++]);
            Assert.That(8 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(10 == volume.Data[index++]);
            Assert.That(8 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(18 == volume.Data[index++]);
            Assert.That(10 == volume.Data[index++]);
            Assert.That(5 == volume.Data[index++]);
            Assert.That(11 == volume.Data[index++]);
            Assert.That(11 == volume.Data[index++]);
            Assert.That(6 == volume.Data[index++]);

            var filter = new double[][]{
            new double[] { 0.06, -0.04, -0.14, -0.34, -0.44, -0.54, -0.74, -0.84, -0.94, -1.54, -1.64, -1.74, -1.94, -2.04, -2.14, -2.34, -2.44, -2.54 },
            new double[] { -0.5, -0.76, -1.02, -1.54, -1.8, -2.06, -2.58, -2.84, -3.1, -4.66, -4.92, -5.18, -5.7, -5.96, -6.22, -6.74, -7, -7.26 } };
           

            for (int i = 0; i < layer.Filters.Count; i++)
            {
                for (int j = 0; j < layer.Filters[i].Data.Length; j++)
                {
                    Assert.That(TestData.AboutEqual(filter[i][j], layer.Filters[i].Data[j]));
                }
            }

            Assert.That(TestData.AboutEqual(-0.1, layer.Bias[0]));
            Assert.That(TestData.AboutEqual(-0.26, layer.Bias[1]));

            Assert.That(true);
        }
    }
}
