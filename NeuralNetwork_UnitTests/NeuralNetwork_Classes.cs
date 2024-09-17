namespace NeuralNetwork_UnitTests
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
        public void MaxPoolingLayer_Process()
        {
            var layer = new MaxPoolingLayer(new VolumeSize(4, 4, 1));
            var volume = layer.Process(new Volume(TestData.TestData_4_4_1, new VolumeSize(4, 4, 1)));

            Assert.That(2 == volume.Size.X);
            Assert.That(2 == volume.Size.Y);
            Assert.That(1 == volume.Size.Z);

            Assert.That(TestData.AboutEqual(6, volume.Data[0]));
            Assert.That(TestData.AboutEqual(8, volume.Data[1]));
            Assert.That(TestData.AboutEqual(14, volume.Data[2]));
            Assert.That(TestData.AboutEqual(16, volume.Data[3]));

            layer = new MaxPoolingLayer(new VolumeSize(4, 4, 2));
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
        public void MaxPoolingLayerBackPropegate()
        {
            var layer = new MaxPoolingLayer(new VolumeSize(4, 4, 2));
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
        }

        [Test]
        public void AveragePoolingLayerBackPropegate()
        {
            var layer = new AveragePoolingLayer(new VolumeSize(4, 4, 2));
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
        }

        [Test]
        public void ReluLayerBackPropegate()
        {
            var layer = new ReluActivationLayer(new VolumeSize(4, 4, 2));
            var error = new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)), error);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(i + 1 == volume.Data[i]);
            }

            volume = layer.BackPropegate(new Volume(TestData.TestData_4_4_2, new VolumeSize(4, 4, 2)) * -1, error);
            for (int i = 0; i < 32; i++)
            {
                Assert.That(0 == volume.Data[i]);
            }
        }

        [Test]
        public void SigmoidLayerBackPropegate()
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
        public void FullyConnectedLayerBackPropegate()
        {
            var layer = new FullyConnectedLayer(1, new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_2_2_1, new VolumeSize(2, 2, 1));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error);
            Assert.That(1 < 0);
        }

        [Test]
        public void ConvolutionLayerBackPropegate()
        {
            var layer = new ConvolutionLayer(1, 3, new VolumeSize(2, 2, 2));
            var error = new Volume(TestData.TestData_2_2_1, new VolumeSize(2, 2, 1));
            var volume = layer.BackPropegate(new Volume(TestData.TestData_2_2_2, new VolumeSize(2, 2, 2)), error);
            Assert.That(1 < 0);
        }
    }
}
