using System.Drawing;
using System.IO;

namespace SpectrumAnalyser
{
    public class BitmapGenerator
    {
        public string PathToFile { get; private set; }
        public Bitmap Image { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        private int currentRow = 0;
        public BitmapGenerator(string path, int width, int height)
        {
            PathToFile = path;
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
        }
        public void SaveImage()
        {
            string name = @$"{ Path.GetDirectoryName(PathToFile)}\{ Path.GetFileNameWithoutExtension(PathToFile) }.bmp";
            Image.Save(name);
        }
    }
}
