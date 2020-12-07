using System;

namespace Spectrogram
{
    public readonly struct AudioData
    {
        public TimeSpan Duration { get; init; }
        public int FftSampleSize { get; init; }
        public long Length { get; init; }
        public int Channels { get; init; }
        public int Exponent { get; init; }
        public int SampleRate { get; init; }
        public string FilePath { get; init; }

        public AudioData(IAudioFile file)
        {
            Duration = file.Duration;
            Exponent = file.Exponent;
            FftSampleSize = file.FftSampleSize;
            FilePath = file.FilePath;
            Length = file.SampleSource.Length;
            Channels = file.SampleSource.WaveFormat.Channels;
            SampleRate = file.SampleSource.WaveFormat.SampleRate;
        }

        public AudioData(TimeSpan duration, int fftSampleSize, long length, int channels, int exponent, int sampleRate, string filePath)
        {
            Duration = duration;
            FftSampleSize = fftSampleSize;
            Length = length;
            Channels = channels;
            Exponent = exponent;
            SampleRate = sampleRate;
            FilePath = filePath;
        }
    }
}
