using CSCore;
using CSCore.Codecs.FLAC;
using CSCore.DSP;
using CSCore.Utils;
using System;

namespace SpectrumAnalyser
{
    public class AudioFile
    {
        public string FilePath { get; set; }
        private FlacFile flacFile;
        public ISampleSource SampleSource { get; private set; }
        private readonly Logger logger = Logger.GetInstance();
        public int BitmapWidth { get; }
        public int FftSampleSize { get; }

        private int exponent = 11;
        private int bitmapRow = 0;
        private int currentPosition = 0;

        public AudioFile(string filePath)
        {
            FilePath = filePath;
            FftSampleSize = (int)Math.Pow(2, exponent);
            Init();
            BitmapWidth = (int)SampleSource.Length / FftSampleSize;
        }

        public void ReadFile(Histogram histogram, BitmapGenerator bitmap)
        {

            float[] samples = new float[FftSampleSize];
            Complex[] complex = new Complex[FftSampleSize];

            PerformFft(exponent, samples, complex);
            SaveResultToHistogram(histogram, FftSampleSize, complex);
            if (currentPosition % 30 == 0)
            {
                bitmap.EditRow(bitmapRow, histogram);
                ++bitmapRow;
                histogram.Data.Clear();
            }
            ++currentPosition;
            //logger.AddLogMessage(LogMessage.LogLevel.Info, $"Done in: {s2 - s1}");
            //TODO Simple Timer
        }



        private void SaveResultToHistogram(Histogram histogram, int fftSampleSize, Complex[] complex)
        {
            for (int i = 0; i < complex.Length / 2; i++)
            {
                double freq = i * SampleSource.WaveFormat.SampleRate / fftSampleSize;
                histogram.Add(freq, complex[i]);
            }
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
            flacFile = new FlacFile(FilePath);
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
