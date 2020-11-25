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
        public TimeSpan Duration { get; }
        ISampleSource SampleSource { get; }

        float[] ReadFile();
        AudioData GetAudioData();
    }
}
