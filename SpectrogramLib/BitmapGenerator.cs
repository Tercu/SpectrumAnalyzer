using System.Drawing;
using System.IO;

namespace Spectrogram
{
    public class BitmapGenerator
    {
        public string PathToFile { get; private set; }
        public Bitmap Image { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        private int currentRow = 0;
        private static Gradient gradient = new Gradient();
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
            foreach (var h in histogram.Data)
            {
                Color color = gradient.GetGradientColor((int)h.Value);
                Image.SetPixel(row, index, color);
                --index;
            }
            currentRow++;
        }
        public void SaveImage()
        {
            char separator = Path.DirectorySeparatorChar;
            string name = @$"{ Path.GetDirectoryName(PathToFile)}{separator}{ Path.GetFileNameWithoutExtension(PathToFile) }.png";
            Image.Save(name);
        }
    }
}
