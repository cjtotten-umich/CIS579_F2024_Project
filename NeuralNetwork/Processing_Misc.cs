namespace NeuralNetwork
{
    using ILGPU;
    using System;
    using ILGPU.Runtime;
    using System.Drawing.Imaging;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public partial class Processing
    {

        static Action<Index1D, ArrayView<byte>, int, int, ArrayView<double>> _kernel_24BPP_RGB_ImageToVolume;
        static Action<Index1D, ArrayView<byte>, int, int, ArrayView<double>> _kernel_32BPP_ARGB_ImageToVolume;

        static void Kernel_24BPP_RGB_ImageToVolume(Index1D index, ArrayView<byte> image, int x, int y, ArrayView<double> volume)
        {
            var size = x * y;
            volume[index + (0 * size)] = image[(index * 3) + 0] / 255f;
            volume[index + (1 * size)] = image[(index * 3) + 1] / 255f;
            volume[index + (2 * size)] = image[(index * 3) + 2] / 255f;
        }

        static void Kernel_32BPP_ARGB_ImageToVolume(Index1D index, ArrayView<byte> image, int x, int y, ArrayView<double> volume)
        {
            var size = x * y;
            volume[index + (0 * size)] = image[(index * 4) + 0] / 255f;
            volume[index + (1 * size)] = image[(index * 4) + 1] / 255f;
            volume[index + (2 * size)] = image[(index * 4) + 2] / 255f;
            volume[index + (3 * size)] = image[(index * 4) + 3] / 255f;
        }

        public static Volume ImageToVolume(Bitmap bitmap)
        {
            if (bitmap.PixelFormat == PixelFormat.Format24bppRgb)
            {
                var bytes = GetBytesFromBitmap(bitmap);
                var imageBuffer = _accelerator.Allocate1D<byte>(bytes.Length);
                var resultBuffer = _accelerator.Allocate1D<double>(bytes.Length);
                imageBuffer.CopyFromCPU(bytes);
                _kernel_24BPP_RGB_ImageToVolume(bitmap.Width * bitmap.Height, imageBuffer.View, bitmap.Width, bitmap.Height, resultBuffer.View);
                var result = resultBuffer.GetAsArray1D();
                return new Volume(result, new VolumeSize(bitmap.Width, bitmap.Height, 3));
            }

            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                var bytes = GetBytesFromBitmap(bitmap);
                var imageBuffer = _accelerator.Allocate1D<byte>(bytes.Length);
                var resultBuffer = _accelerator.Allocate1D<double>(bytes.Length);
                imageBuffer.CopyFromCPU(bytes);
                _kernel_32BPP_ARGB_ImageToVolume(bitmap.Width * bitmap.Height, imageBuffer.View, bitmap.Width, bitmap.Height, resultBuffer.View);
                var result = resultBuffer.GetAsArray1D();
                return new Volume(result, new VolumeSize(bitmap.Width, bitmap.Height, 4));
            }

            throw new ArgumentException("New Pixel Format");
        }

        public static Bitmap VolumeToImage(Volume volume)
        {
            if (volume.Size.Z > 1)
            {
                throw new ArgumentException("Not done yet");
            }

            var image = new Bitmap(volume.Size.X, volume.Size.Y, PixelFormat.Format24bppRgb);
            for (int x = 0; x < volume.Size.X; x++)
            {
                for (int y = 0; y < volume.Size.Y; y++)
                {
                    var c = (int)Math.Min((volume.Data[y * volume.Size.X + x] * 255), 255);
                    image.SetPixel(x, y, Color.FromArgb(1, c, c, c));
                }
            }

            return image;
        }

        public static Bitmap GetBitmapFromBytes(byte[] bytes, int width, int height)
        {
            if (bytes == null)
            {
                return null;
            }

            var image = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            Marshal.Copy(bytes, 0, imageData.Scan0, bytes.Length);
            image.UnlockBits(imageData);
            return image;
        }

        public static byte[] GetBytesFromBitmap(Bitmap image)
        {
            var pixelSize = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            var imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            var data = new byte[pixelSize * imageData.Width * imageData.Height];
            for (int i = 0; i < imageData.Height; i++)
            {
                Marshal.Copy(imageData.Scan0 + (i * imageData.Stride), data, pixelSize * imageData.Width * i, pixelSize * imageData.Width);
            }

            image.UnlockBits(imageData);
            return data;
        }

        static void Convolve(Index1D index, ArrayView<double> m1, ArrayView<double> m2, int m1x, int m1y, int m1z, int m2x, int m2y, int m2z, ArrayView<double> result)
        {
            int x = index % (m1x - m2x + 1);
            int y = index / (m1x - m2x + 1);

            double sum = 0;
            for (int i = 0; i < m2z; i++)
            {
                for (int j = 0; j < m2y; j++)
                {
                    for (int k = 0; k < m2x; k++)
                    {
                        var m1i = x + (y * m1x) + k + (m1x * j) + (m1x * m1y * i);
                        var m2i = k + (j * m2x) + (i * m2x * m2y);
                        sum += m1[m1i] * m2[m2i];
                    }
                }
            }

            result[index] = sum;
        }

        public static Tuple<double, double, double> Adam(double previousMomentum, double previousVariance, int trainingStep, double gradient)
        {
            var momentumDecay = 0.9;
            var varianceDecay = 0.999;
            var trainingRate = 0.001;
            var epsilon = 0.00000001;

            var momentum = (momentumDecay * previousMomentum) + ((1 - momentumDecay) * gradient);
            var variance = (varianceDecay * previousVariance) + ((1 - varianceDecay) * gradient * gradient);

            var m = momentum / (1.0 - Math.Pow(momentumDecay, trainingStep));
            var v = variance / (1.0 - Math.Pow(varianceDecay, trainingStep));

            var d = (trainingRate * m) / (Math.Sqrt(v + epsilon));
            return Tuple.Create(momentum, variance, d);
        }
    }
}
