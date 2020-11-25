using System;
using System.Drawing;

namespace Spectrogram
{
    public interface IBitmapGenerator : IDisposable
    {
        Bitmap Image { get; }

        void EditRow(int row, Histogram histogram);
        void SaveImage();
    }
}
