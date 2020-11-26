using System;

namespace Spectrogram
{
    public struct AudioData
    {
        public readonly TimeSpan Duration { get; init; }
        public readonly int FftSampleSize { get; init; }
        public readonly long Length { get; init; }
        public readonly int Channels { get; init; }
        public readonly int Exponent { get; init; }
        public readonly int SampleRate { get; init; }
        public readonly string FilePath { get; init; }

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
