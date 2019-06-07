using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TensorFlow;

namespace ObjectDetection.Utilities
{
    public static class TensorUtil
    {
        public static TFTensor CreateFromImageFile(Stream stream, TFDataType destinationDataType = TFDataType.Float)
        {
            unsafe
            {
                var bitmap = new Bitmap(stream);

                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb);

                var matrix = new byte[1, bitmap.Height, bitmap.Width, 3];

                var scan0 = (byte*)data.Scan0.ToPointer();

                for (var i = 0; i < data.Height; ++i)
                {
                    for (var j = 0; j < data.Width; ++j)
                    {
                        var pixelData = scan0 + i * data.Stride + j * 3;
                        matrix[0, i, j, 0] = pixelData[2];
                        matrix[0, i, j, 1] = pixelData[1];
                        matrix[0, i, j, 2] = pixelData[0];
                    }
                }

                bitmap.UnlockBits(data);

                TFTensor tensor = matrix;
                return tensor;
            }
        }
    }
}