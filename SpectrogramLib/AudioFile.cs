using CSCore;
using CSCore.Codecs;
using System;

namespace Spectrogram
{
    public class AudioFile : IAudioFile
    {
        public string FilePath { get; set; }
        public ISampleSource SampleSource { get; private set; }
        private readonly Logger logger = Logger.GetInstance();
        public int BitmapWidth { get; }
        public int FftSampleSize { get; }
        public int Exponent { get; }


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
            BitmapWidth = (int)SampleSource.Length / FftSampleSize;
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
    }
}
