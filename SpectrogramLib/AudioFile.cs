using CSCore;
using CSCore.Codecs;
using System;

namespace Spectrogram
{
    public class AudioFile : IAudioFile
    {
        public string FilePath { get; init; }
        public ISampleSource SampleSource { get; init; }
        public int FftSampleSize { get; init; }
        public int Exponent { get; init; }
        public TimeSpan Duration { get { return SampleSource.GetLength(); } }

        private readonly Logger logger = Logger.GetInstance();
        private bool disposedValue;



        public AudioFile(string filePath, int exponent = 11, Boolean mono = false)
        {
            Exponent = exponent;
            FilePath = filePath;
            SampleSource = CodecFactory.Instance.GetCodec(FilePath).ToSampleSource();
            if (mono)
            {
                SampleSource = SampleSource.ToMono();
            }
            FftSampleSize = (int)Math.Pow(2, Exponent);
        }
        public AudioFile(ISampleSource source, int exponent = 11, Boolean mono = false)
        {
            Exponent = exponent;
            SampleSource = source;
            FftSampleSize = (int)Math.Pow(2, Exponent);
            if (mono)
            {
                SampleSource = SampleSource.ToMono();
            }
        }

        public long ReadFile(out float[] sample)
        {
            sample = new float[FftSampleSize];
            long shift = SampleSource.Read(sample, 0, sample.Length);
            return shift;
        }

        public AudioData GetAudioData()
        {
            AudioData data = new AudioData(this);
            return data;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    return;
                }
                SampleSource?.Dispose();

                disposedValue = true;
            }
        }

        ~AudioFile()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
