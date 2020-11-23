using System;
using System.Drawing;

namespace Spectrogram
{
    public interface IBitmapGenerator : IDisposable
    {
        Bitmap Image { get; }
        string PathToFile { get; }

        void EditRow(int row, Histogram histogram);
        void SaveImage();
    }
}
