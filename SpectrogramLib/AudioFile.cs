using CSCore;
using CSCore.Codecs;
using System;

namespace Spectrogram
{
    public class AudioFile : IAudioFile
    {
        public string FilePath { get; init; }
        public ISampleSource SampleSource { get; init; }
        private readonly Logger logger = Logger.GetInstance();
        public int FftSampleSize { get; init; }
        public int Exponent { get; init; }
        public TimeSpan Duration { get { return SampleSource.GetLength(); } }


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

        public float[] ReadFile()
        {
            float[] samples = new float[FftSampleSize];
            SampleSource.Read(samples, 0, samples.Length);

            return samples;
        }

        public void Dispose()
        {
            SampleSource.Dispose();
        }

        public AudioData GetAudioData()
        {
            AudioData data = new AudioData(this);
            return data;
        }
    }
}
