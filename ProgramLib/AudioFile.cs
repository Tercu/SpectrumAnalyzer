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
        private readonly Logger logger = Logger.GetInstance();
        public Histogram Histogram { get; private set; }

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            Init();
            Histogram = new Histogram();
        }

        public void ReadFile()
        {
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(FilePath)}");
            int exponent = 11;
            int fftSampleSize = (int)Math.Pow(2, exponent);
            float[] samples = new float[fftSampleSize];
            Complex[] complex = new Complex[fftSampleSize];
            sampleSource.Position = fftSampleSize;
            do
            {
                PerformFft(exponent, samples, complex);
                SaveResultToHistogram(fftSampleSize, complex);
            }
            while (sampleSource.Length - fftSampleSize >= sampleSource.Position);
            //logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
            //TODO Simple Timer
        }

        private void SaveResultToHistogram(int fftSampleSize, Complex[] complex)
        {
            for (int i = 0; i < complex.Length / 2; i++)
            {
                double result = Math.Sqrt(Math.Pow(complex[i].Real, 2) + Math.Pow(complex[i].Imaginary, 2));
                double freq = i * sampleSource.WaveFormat.SampleRate / fftSampleSize;
                Histogram.Add(freq, result);
            }
        }

        private void PerformFft(int exponent, float[] samples, Complex[] complex)
        {
            sampleSource.Read(samples, 0, samples.Length);
            FillComplexArrayRealOnly(samples, complex);
            FastFourierTransformation.Fft(complex, exponent, FftMode.Forward);
        }

        private void Init()
        {
            flacFile = new FlacFile(FilePath);
            sampleSource = flacFile.ToSampleSource().ToMono();
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
