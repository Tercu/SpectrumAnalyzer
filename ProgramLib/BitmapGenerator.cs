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
        private Gradient gradient = new Gradient();
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
                Color color = gradient.GetGradientColor(255 - (int)(h.Value * 255));
                Image.SetPixel(row, index, color);
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
