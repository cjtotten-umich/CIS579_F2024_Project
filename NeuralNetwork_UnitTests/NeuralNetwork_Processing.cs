namespace NeuralNetwork_UnitTests
{
    using NeuralNetwork;
    using NUnit.Framework;
    using System.Drawing;

    public class NeuralNetwork_Processing
    {
        [Test]
        public void Processing_Convolve()
        {
            var volume = Processing.Convolve(new Volume(TestData.TestData_4_4_3, new VolumeSize(4, 4, 3)), new Volume(TestData.TestKernel_3_3_3, new VolumeSize(3, 3, 3)), 0);
            Assert.That(16 == volume.Data.Length);
            Assert.That(4 == volume.Size.X);
            Assert.That(4 == volume.Size.Y);
            Assert.That(1 ==  volume.Size.Z);
            Assert.That(TestData.AboutEqual(volume.Data[5], 11142));
            Assert.That(TestData.AboutEqual(volume.Data[10], 13032));
            Assert.That(TestData.AboutEqual(volume.Data[0], 4935));
            Assert.That(TestData.AboutEqual(volume.Data[15], 5439));

            volume = Processing.Convolve(new Volume(TestData.TestData_4_4_3, new VolumeSize(4, 4, 3)), new Volume(TestData.TestKernel_3_3_3, new VolumeSize(3, 3, 3)), 1);
            Assert.That(16 == volume.Data.Length);
            Assert.That(4 == volume.Size.X);
            Assert.That(4 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);
            Assert.That(TestData.AboutEqual(volume.Data[5], 11143));
            Assert.That(TestData.AboutEqual(volume.Data[10], 13033));
            Assert.That(TestData.AboutEqual(volume.Data[0], 4936));
            Assert.That(TestData.AboutEqual(volume.Data[15], 5440));
        }

        [Test]
        public void Processing_ImageToVolume()
        {
            var image = new Bitmap(2, 2, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            image.SetPixel(0, 0, Color.FromArgb(255, 1, 2, 3));
            image.SetPixel(1, 0, Color.FromArgb(255, 4, 5, 6)); 
            image.SetPixel(0, 1, Color.FromArgb(255, 7, 8, 9));
            image.SetPixel(1, 1, Color.FromArgb(255, 10, 11, 12));
            var volume = Processing.ImageToVolume(image);
            Assert.That(12 == volume.Data.Length);
            Assert.That(3 / 255f == volume.Data[0]);
            Assert.That(6 / 255f == volume.Data[1]);
            Assert.That(9 / 255f == volume.Data[2]);
            Assert.That(12 / 255f == volume.Data[3]);
            Assert.That(2 / 255f == volume.Data[4]);
            Assert.That(5 / 255f == volume.Data[5]);
            Assert.That(8 / 255f == volume.Data[6]);
            Assert.That(11 / 255f == volume.Data[7]);
            Assert.That(1 / 255f == volume.Data[8]);
            Assert.That(4 / 255f == volume.Data[9]);
            Assert.That(7 / 255f == volume.Data[10]);
            Assert.That(10 / 255f == volume.Data[11]);

            image = new Bitmap(2, 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            image.SetPixel(0, 0, Color.FromArgb(255, 1, 2, 3));
            image.SetPixel(1, 0, Color.FromArgb(255, 4, 5, 6));
            image.SetPixel(0, 1, Color.FromArgb(255, 7, 8, 9));
            image.SetPixel(1, 1, Color.FromArgb(255, 10, 11, 12));
            volume = Processing.ImageToVolume(image);
            Assert.That(16 == volume.Data.Length);
            Assert.That(3 / 255f == volume.Data[0]);
            Assert.That(6 / 255f == volume.Data[1]);
            Assert.That(9 / 255f == volume.Data[2]);
            Assert.That(12 / 255f == volume.Data[3]);
            Assert.That(2 / 255f == volume.Data[4]);
            Assert.That(5 / 255f == volume.Data[5]);
            Assert.That(8 / 255f == volume.Data[6]);
            Assert.That(11 / 255f == volume.Data[7]);
            Assert.That(1 / 255f == volume.Data[8]);
            Assert.That(4 / 255f == volume.Data[9]);
            Assert.That(7 / 255f == volume.Data[10]);
            Assert.That(10 / 255f == volume.Data[11]);
            Assert.That(255 / 255f == volume.Data[12]);
            Assert.That(255 / 255f == volume.Data[13]);
            Assert.That(255 / 255f == volume.Data[14]);
            Assert.That(255 / 255f == volume.Data[15]);
        }

        [Test]
        public void Processing_MaxPool()
        {
            var originalVolume = new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1));
            var volume = Processing.MaxPool(originalVolume);
            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(6, volume.Data[0]));
            Assert.That(TestData.AboutEqual(8, volume.Data[1]));
            Assert.That(TestData.AboutEqual(14, volume.Data[2]));
            Assert.That(TestData.AboutEqual(16, volume.Data[3]));

            originalVolume = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            volume = Processing.MaxPool(originalVolume);
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
        public void Processing_FullyConnected()
        {
            var originalVolume = new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2));
            var weights = new Volume(TestData.TestWeights_2_2_2_2, new VolumeSize(4, 2, 2));
            var volume = Processing.FullyConnected(originalVolume, weights, Volume.MakeZero(new VolumeSize(2, 1, 1)));
            Assert.That(2 == volume.Size.X);
            Assert.That(1 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(204, volume.Data[0]));
            Assert.That(TestData.AboutEqual(492, volume.Data[1]));

            volume = Processing.FullyConnected(originalVolume, weights, Volume.MakeZero(new VolumeSize(2, 1, 1)) + 1);
            Assert.That(2 == volume.Size.X);
            Assert.That(1 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(205, volume.Data[0]));
            Assert.That(TestData.AboutEqual(493, volume.Data[1]));
        }

        [Test]
        public void Processing_AveragePool()
        {
            var originalVolume = new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1));
            var volume = Processing.AveragePool(originalVolume);
            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);   
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(3.5f, volume.Data[0]));
            Assert.That(TestData.AboutEqual(5.5f, volume.Data[1]));
            Assert.That(TestData.AboutEqual(11.5f, volume.Data[2]));
            Assert.That(TestData.AboutEqual(13.5f, volume.Data[3]));

            originalVolume = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            volume = Processing.AveragePool(originalVolume);
            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);
            Assert.That(2 == volume.Size.Z);

            Assert.That((14.0 / 4) == volume.Data[0]);
            Assert.That((22.0 / 4) == volume.Data[1]);
            Assert.That((46.0 / 4) == volume.Data[2]);
            Assert.That((54.0 / 4) == volume.Data[3]);

            Assert.That((78.0 / 4) == volume.Data[4]);
            Assert.That((86.0 / 4) == volume.Data[5]);
            Assert.That((110.0 / 4) == volume.Data[6]);
            Assert.That((118.0 / 4) == volume.Data[7]);
        }

        [Test]
        public void Processing_Adam()
        {
            var a = Processing.Adam(1.0, 1.0, 10, 0.1);
            Assert.That(TestData.AboutEqual(0.91, a.Item1));
            Assert.That(TestData.AboutEqual(0.99901, a.Item2));
            Assert.That(TestData.AboutEqual(0.0001394702, a.Item3));
            a = Processing.Adam(a.Item1, a.Item2, 11, 0.1);
            Assert.That(TestData.AboutEqual(0.829, a.Item1));
            Assert.That(TestData.AboutEqual(0.99802099, a.Item2));
            Assert.That(TestData.AboutEqual(0.00012651785942637667, a.Item3));
        }

        [Test]
        public void Processing_MeanSquaredError()
        {
            var data1 = Volume.MakeZero(new VolumeSize(2, 2, 2));
            var data2 = data1 + 5;

            var a = Processing.MeanSquareError(data1, data2);
            Assert.That(25 == a);
        }

        [Test]
        public void Processing_Relu()
        {
            var data = Volume.MakeZero(new VolumeSize(2, 2, 2));
            data += 1;
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(1 == data.Data[i]);
            }

            data = Processing.Relu(data);
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(1 == data.Data[i]);
            }

            data *= -1;
            data = Processing.Relu(data);
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(0 == data.Data[i]);
            }
        }

        [Test]
        public void Processing_Sigmoid()
        {
            var data = Volume.MakeZero(new VolumeSize(2, 2, 2));
            data += 1;
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(1 == data.Data[i]);
            }

            data = Processing.Sigmoid(data);
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(TestData.AboutEqual(0.731058578, data.Data[i]));
            }

            data = Volume.MakeZero(new VolumeSize(2, 2, 2));
            data -= 1;
            data = Processing.Sigmoid(data);
            for (int i = 0; i < data.Size.TotalSize; i++)
            {
                Assert.That(TestData.AboutEqual(0.268941421, data.Data[i]));
            }
        }
    }
}