using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.Utils;
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


        public AudioFile(string filePath, int exponent = 11)
        {
            Exponent = exponent;
            FilePath = filePath;
            SampleSource = CodecFactory.Instance.GetCodec(FilePath).ToSampleSource().ToMono();
            FftSampleSize = (int)Math.Pow(2, Exponent);
            BitmapWidth = (int)SampleSource.Length / FftSampleSize;
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
