using CSCore;
using CSCore.Codecs.FLAC;
using CSCore.DSP;
using CSCore.Utils;
using System;
using System.IO;

namespace SpectrumAnalyser
{
    public class AudioFile
    {
        public string FilePath { get; set; }
        private FlacFile flacFile;
        private ISampleSource sampleSource;
        private Logger logger = Logger.GetInstance();

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            Init();
        }

        public void ReadFile()
        {
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(FilePath)}");
            int exponent = 8;
            int fftSampleSize = (int)Math.Pow(2, exponent);
            float[] samples = new float[fftSampleSize];
            Complex[] complex = new Complex[(fftSampleSize)];
            do
            {
                PerformFft(exponent, samples, complex);
            } while (sampleSource.Length - fftSampleSize >= sampleSource.Position);
            //logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
            //TODO Simple Timer
        }

        private void PerformFft(int exponent, float[] samples, Complex[] complex)
        {
            sampleSource.Read(samples, 0, samples.Length);
            FillComplexArray(samples, complex);
            FastFourierTransformation.Fft(complex, exponent, FftMode.Forward);
        }

        private void Init()
        {
            flacFile = new FlacFile(FilePath);
            sampleSource = flacFile.ToSampleSource();
        }

        private void FillComplexArray(float[] samples, Complex[] complex)
        {
            for (int i = 0; i < samples.Length / 2 - 1; ++i)
            {
                complex[i].Real = samples[2 * i];
                complex[i].Imaginary = samples[2 * i + 1];
            }
        }
    }
}
