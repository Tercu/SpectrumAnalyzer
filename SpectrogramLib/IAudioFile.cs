using CSCore;
using CSCore.Utils;
using System;

namespace Spectrogram
{
    public interface IAudioFile : IDisposable
    {
        int FftSampleSize { get; }
        string FilePath { get; }
        public int Exponent { get; }
        ISampleSource SampleSource { get; }

        float[] ReadFile();
    }
}
