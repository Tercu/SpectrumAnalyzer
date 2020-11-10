using System.Drawing;

namespace SpectrumAnalyser
{
    public class BitmapGenerator
    {
        public Bitmap Image { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        private int currentRow = 0;
        public BitmapGenerator(int width, int height)
        {
            Height = height;
            Width = width;
            Image = new Bitmap(Width, Height);

        }
        public void EditRow(int row, Histogram histogram)
        {
            int index = Height - 1;
            histogram.Normalize();
            foreach (var h in histogram.NormalizedData)
            {
                double red = h.Value * 255;
                Image.SetPixel(row, index, Color.FromArgb(255, 255 - (int)red, 255 - (int)red));
                --index;
            }
            currentRow++;
            SaveImage("");
        }
        public void SaveImage(string path)
        {
            path = @"D:\dev\testImage.bmp";
            Image.Save(path);
        }
    }
}
