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
        private BitmapGenerator bitmap;
        public Histogram Histogram { get; private set; }

        private int exponent = 11;
        private int bitmapRow = 0;
        private int fftSampleSize;
        private int bitmapWidth;

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            fftSampleSize = (int)Math.Pow(2, exponent);
            Init();
            bitmapWidth = (int)sampleSource.Length / fftSampleSize;
            bitmap = new BitmapGenerator(FilePath, bitmapWidth / 30 + 10, 1025);
            Histogram = new Histogram();
        }

        public void ReadFile()
        {
            logger.AddLogMessage(LogMessage.LogLevel.Info, $"Processing file: {Path.GetFileName(FilePath)}");

            float[] samples = new float[fftSampleSize];
            Complex[] complex = new Complex[fftSampleSize];
            int i = 0;
            do
            {
                PerformFft(exponent, samples, complex);
                SaveResultToHistogram(fftSampleSize, complex);
                if (i % 30 == 0)
                {
                    bitmap.EditRow(bitmapRow, Histogram);
                    ++bitmapRow;
                    Histogram.Data.Clear();
                }
                ++i;
            }
            while (sampleSource.Position < sampleSource.Length - fftSampleSize);
            bitmap.SaveImage();
            //logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
            //TODO Simple Timer
        }

        private void SaveResultToHistogram(int fftSampleSize, Complex[] complex)
        {
            for (int i = 0; i < complex.Length / 2; i++)
            {
                double freq = i * sampleSource.WaveFormat.SampleRate / fftSampleSize;
                Histogram.Add(freq, complex[i]);
            }
        }

        private void PerformFft(int exponent, float[] samples, Complex[] complex)
        {
            sampleSource.Read(samples, 0, samples.Length);
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] *= (float)FastFourierTransformation.HammingWindow(i, samples.Length);
            }
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
