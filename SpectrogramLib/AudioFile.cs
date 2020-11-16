using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.Utils;
using System;

namespace Spectrogram
{
    public class AudioFile
    {
        public string FilePath { get; set; }
        private IWaveSource flacFile;
        public ISampleSource SampleSource { get; private set; }
        private readonly Logger logger = Logger.GetInstance();
        public int BitmapWidth { get; }
        public int FftSampleSize { get; }

        private int exponent = 11;

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            FftSampleSize = (int)Math.Pow(2, exponent);
            Init();
            BitmapWidth = (int)SampleSource.Length / FftSampleSize;
        }

        public Complex[] ReadFile()
        {
            float[] samples = new float[FftSampleSize];
            Complex[] complex = new Complex[FftSampleSize];

            PerformFft(exponent, samples, complex);
            return complex;
        }

        private void PerformFft(int exponent, float[] samples, Complex[] complex)
        {
            SampleSource.Read(samples, 0, samples.Length);
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] *= (float)FastFourierTransformation.HammingWindow(i, samples.Length);
            }
            FillComplexArrayRealOnly(samples, complex);
            FastFourierTransformation.Fft(complex, exponent, FftMode.Forward);
        }

        private void Init()
        {
            IWaveSource flacFile = CodecFactory.Instance.GetCodec(FilePath);
            SampleSource = flacFile.ToSampleSource().ToMono();
        }
        private void FillComplexArrayRealOnly(float[] samples, Complex[] complex)
        {
            for (int i = 0; i < samples.Length; ++i)
            {
                complex[i].Real = samples[i];
                complex[i].Imaginary = 0;
            }
        }
    }
}
