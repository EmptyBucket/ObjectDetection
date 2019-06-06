using System;
using System.Drawing;

namespace ObjectDetection.Utilities
{
    public class ImageEditor : IDisposable
    {
        private readonly string fontFamily;
        private readonly float fontSize;
        private readonly Graphics graphics;
        private readonly Image image;
        private readonly string outputFile;

        public ImageEditor(string inputFile, string outputFile, string fontFamily = "Ariel", float fontSize = 12)
        {
            if (string.IsNullOrEmpty(inputFile))
            {
                throw new ArgumentNullException(nameof(inputFile));
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentNullException(nameof(outputFile));
            }

            this.fontFamily = fontFamily;
            this.fontSize = fontSize;
            this.outputFile = outputFile;

            image = Image.FromFile(inputFile);
            graphics = Graphics.FromImage(image);
        }

        public void AddBox(float xmin, float xmax, float ymin, float ymax, string text = "", string colorName = "red")
        {
            var left = xmin * image.Width;
            var right = xmax * image.Width;
            var top = ymin * image.Height;
            var bottom = ymax * image.Height;

            var imageRectangle = new Rectangle(new Point(0, 0), new Size(image.Width, image.Height));
            graphics.DrawImage(image, imageRectangle);

            var color = Color.FromName(colorName);
            Brush brush = new SolidBrush(color);
            var pen = new Pen(brush);

            graphics.DrawRectangle(pen, left, top, right - left, bottom - top);
            var font = new Font(fontFamily, fontSize);
            var size = graphics.MeasureString(text, font);
            graphics.DrawString(text, font, brush, new PointF(left, top - size.Height));
        }

        public void Dispose()
        {
            if (image != null)
            {
                image.Save(outputFile);

                graphics?.Dispose();

                image.Dispose();
            }
        }
    }
}