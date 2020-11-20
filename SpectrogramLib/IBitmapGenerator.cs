using System;
using System.Drawing;

namespace Spectrogram
{
    public interface IBitmapGenerator : IDisposable
    {
        int Height { get; }
        Bitmap Image { get; }
        string PathToFile { get; }
        int Width { get; }

        void EditRow(int row, Histogram histogram);
        void SaveImage();
    }
}
